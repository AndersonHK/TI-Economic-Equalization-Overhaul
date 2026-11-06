#include <algorithm>
#include <cmath>
#include <cstdint>

// Blittable ABI, checked against every managed record at load and in validation.
// Ownership stays with Unity's job containers. The kernel neither allocates nor
// touches Unity objects and is reentrant across disjoint flight ranges.
struct V { float x,y,z; float operator[](int i) const { return i==0?x:i==1?y:z; } };
static V add(V a,V b){return {a.x+b.x,a.y+b.y,a.z+b.z};}
static V sub(V a,V b){return {a.x-b.x,a.y-b.y,a.z-b.z};}
static V mul(V a,float s){return {a.x*s,a.y*s,a.z*s};}
static V lo(V a,V b){return {std::min(a.x,b.x),std::min(a.y,b.y),std::min(a.z,b.z)};}
static V hi(V a,V b){return {std::max(a.x,b.x),std::max(a.y,b.y),std::max(a.z,b.z)};}
static float dot(V a,V b){return a.x*b.x+a.y*b.y+a.z*b.z;}
static V cross(V a,V b){return {a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x};}
static V norm(V a){float d=std::sqrt(dot(a,a));return d>1e-5f?V{a.x/d,a.y/d,a.z/d}:V{};}
struct M {float a[16];};
static M inverted(const M&m){
    const double a=m.a[0],b=m.a[4],c=m.a[8],d=m.a[1],e=m.a[5],f=m.a[9],g=m.a[2],h=m.a[6],i=m.a[10];
    const double det=a*(e*i-f*h)-b*(d*i-f*g)+c*(d*h-e*g);M r{};if(det==0)return r;
    r.a[0]=static_cast<float>((e*i-f*h)/det);r.a[4]=static_cast<float>((c*h-b*i)/det);r.a[8]=static_cast<float>((b*f-c*e)/det);
    r.a[1]=static_cast<float>((f*g-d*i)/det);r.a[5]=static_cast<float>((a*i-c*g)/det);r.a[9]=static_cast<float>((c*d-a*f)/det);
    r.a[2]=static_cast<float>((d*h-e*g)/det);r.a[6]=static_cast<float>((b*g-a*h)/det);r.a[10]=static_cast<float>((a*e-b*d)/det);
    r.a[12]=-(r.a[0]*m.a[12]+r.a[4]*m.a[13]+r.a[8]*m.a[14]);
    r.a[13]=-(r.a[1]*m.a[12]+r.a[5]*m.a[13]+r.a[9]*m.a[14]);
    r.a[14]=-(r.a[2]*m.a[12]+r.a[6]*m.a[13]+r.a[10]*m.a[14]);r.a[15]=1;return r;
}
static V vector(const M&m,V p){return {m.a[0]*p.x+m.a[4]*p.y+m.a[8]*p.z,m.a[1]*p.x+m.a[5]*p.y+m.a[9]*p.z,m.a[2]*p.x+m.a[6]*p.y+m.a[10]*p.z};}
static V point(const M&m,V p){return add(vector(m,p),{m.a[12],m.a[13],m.a[14]});}
static V normal(const M&m,V n){return norm({m.a[0]*n.x+m.a[1]*n.y+m.a[2]*n.z,m.a[4]*n.x+m.a[5]*n.y+m.a[6]*n.z,m.a[8]*n.x+m.a[9]*n.y+m.a[10]*n.z});}
struct Bounds {V center,extents;};
struct Node {V min,max;int start,count,skip,envelopeStart,envelopeCount;};
struct Vertex {V point;int influenceStart,influenceCount;};
struct Influence {V point;float weight;int bone;};
struct Envelope {V min,max;int bone;};
struct Surface {int root,pose,body,faction,layer,kind,live,generation;Bounds primitive;};
struct Flight {V previous,origin;int pose,parent,body,faction,mask,live;float maximum;};
struct Result {V start,end,next,point,normal;float distance;int surface,expired,generation;};
struct RayInput {V origin,direction;float maximum;int mask;};
struct WorldNode {V min,max;int start,count,skip;};
struct View {
    const Node*geometry;const Vertex*vertices;const int*indices;const Influence*influences;
    const Envelope*envelopes;const Surface*surfaces;const M*world;const M*inverse;
    const Flight*flights;Result*results;Bounds*bounds;int*order;WorldNode*nodes;
};
static Bounds transformed(V min,V max,const M&m){
    V e=mul(sub(max,min),.5f);
    return {point(m,mul(add(min,max),.5f)),{
        std::abs(m.a[0])*e.x+std::abs(m.a[4])*e.y+std::abs(m.a[8])*e.z,
        std::abs(m.a[1])*e.x+std::abs(m.a[5])*e.y+std::abs(m.a[9])*e.z,
        std::abs(m.a[2])*e.x+std::abs(m.a[6])*e.y+std::abs(m.a[10])*e.z}};
}
static V bmin(Bounds b){return b.extents.x<0?V{1,1,1}:sub(b.center,b.extents);}
static V bmax(Bounds b){return b.extents.x<0?V{-1,-1,-1}:add(b.center,b.extents);}
static Bounds united(Bounds a,Bounds b){if(a.extents.x<0)return b;if(b.extents.x<0)return a;V min=lo(bmin(a),bmin(b)),max=hi(bmax(a),bmax(b));return {mul(add(min,max),.5f),mul(sub(max,min),.5f)};}
static Bounds nodeBounds(const View&v,const Node&n,int pose){
    if(n.envelopeCount==0)return transformed(n.min,n.max,v.world[pose]);
    const Envelope&e=v.envelopes[n.envelopeStart];Bounds b=transformed(e.min,e.max,v.world[pose+e.bone]);
    for(int i=1;i<n.envelopeCount;i++){const Envelope&q=v.envelopes[n.envelopeStart+i];b=united(b,transformed(q.min,q.max,v.world[pose+q.bone]));}return b;
}
struct Ray {
    V o,d;double reciprocal[3];bool parallel[3];
    Ray(V origin,V direction):o(origin),d(direction){for(int i=0;i<3;i++){parallel[i]=std::abs(d[i])<1e-20f;reciprocal[i]=parallel[i]?0:1.0/d[i];}}
    bool box(V min,V max,float maximum)const{
        if(min.x>max.x)return false;
        double near=0,far=maximum;
        for(int i=0;i<3;i++){
            if(parallel[i]){if(o[i]<min[i]||o[i]>max[i])return false;continue;}
            double a=(min[i]-static_cast<double>(o[i]))*reciprocal[i],b=(max[i]-static_cast<double>(o[i]))*reciprocal[i];
            if(a>b)std::swap(a,b);if(a>far||b<near)return false;near=std::max(near,a);far=std::min(far,b);
        }return maximum>=0;
    }
};
static bool triangle(const Ray&r,V a,V b,V c,float maximum,float&distance,V&n){
    V e1=sub(b,a),e2=sub(c,a),p=cross(r.d,e2);
    double det=static_cast<double>(e1.x)*p.x+static_cast<double>(e1.y)*p.y+static_cast<double>(e1.z)*p.z;
    if(!std::isfinite(det)||std::abs(det)<1e-20)return false;V s=sub(r.o,a);
    double u=(static_cast<double>(s.x)*p.x+static_cast<double>(s.y)*p.y+static_cast<double>(s.z)*p.z)/det;if(u<0||u>1)return false;
    V q=cross(s,e1);double w=(static_cast<double>(r.d.x)*q.x+static_cast<double>(r.d.y)*q.y+static_cast<double>(r.d.z)*q.z)/det;
    if(w<0||u+w>1)return false;
    double t=(static_cast<double>(e2.x)*q.x+static_cast<double>(e2.y)*q.y+static_cast<double>(e2.z)*q.z)/det;
    if(t<0||t>maximum)return false;distance=static_cast<float>(t);n=norm(cross(e1,e2));if(dot(n,r.d)>0)n=mul(n,-1);return true;
}
static V posed(const View&v,const Vertex&p,int pose){V result{};for(int i=0;i<p.influenceCount;i++){const Influence&inf=v.influences[p.influenceStart+i];result=add(result,mul(point(v.world[pose+inf.bone],inf.point),inf.weight));}return result;}
static bool primitive(const Surface&s,V o,V d,float maximum,float&distance,V&n){
    if(s.kind==3){
        V offset=sub(o,s.primitive.center);double dx=d.x,dy=d.y,dz=d.z,ox=offset.x,oy=offset.y,oz=offset.z;
        double a=dx*dx+dy*dy+dz*dz;if(!std::isfinite(a)||a<=0)return false;
        double cx=oy*dz-oz*dy,cy=oz*dx-ox*dz,cz=ox*dy-oy*dx;
        double radius=s.primitive.extents.x,radial=radius*radius-(cx*cx+cy*cy+cz*cz)/a;if(radial<0)return false;
        double middle=-(ox*dx+oy*dy+oz*dz)/a,half=std::sqrt(radial/a),t=middle-half;if(t<0)t=middle+half;
        if(!std::isfinite(t)||t<0||t>maximum)return false;distance=static_cast<float>(t);n=norm(add(offset,mul(d,distance)));return true;
    }
    double near=0,far=maximum;V entry{},exit{};V min=bmin(s.primitive),max=bmax(s.primitive);
    for(int axis=0;axis<3;axis++){
        if(std::abs(d[axis])<1e-20f){if(o[axis]<min[axis]||o[axis]>max[axis])return false;continue;}
        double a=(min[axis]-o[axis])/static_cast<double>(d[axis]),b=(max[axis]-o[axis])/static_cast<double>(d[axis]);
        V face=axis==0?V{-1,0,0}:axis==1?V{0,-1,0}:V{0,0,-1};
        if(a>b){std::swap(a,b);face=mul(face,-1);}if(a>=near){near=a;entry=face;}if(b<=far){far=b;exit=mul(face,-1);}if(near>far)return false;
    }
    if(dot(entry,entry)==0){if(dot(exit,exit)==0||far<0||far>maximum)return false;distance=static_cast<float>(far);n=exit;}
    else{distance=static_cast<float>(near);n=entry;}return true;
}
static bool intersect(const View&v,const Surface&s,const Ray&worldRay,float maximum,float&distance,V&hitNormal){
    M inv=s.kind==1?M{}:inverted(v.world[s.pose]);distance=maximum;bool mixed=s.kind==1,found=false;
    Ray ray(mixed?worldRay.o:point(inv,worldRay.o),mixed?worldRay.d:vector(inv,worldRay.d));
    if(s.kind>=2){V n{};if(!primitive(s,ray.o,ray.d,maximum,distance,n))return false;hitNormal=normal(inv,n);return true;}
    int end=v.geometry[s.root].skip;
    for(int i=s.root;i<end;){
        const Node&node=v.geometry[i];V min=node.min,max=node.max;
        if(mixed){Bounds b=nodeBounds(v,node,s.pose);min=bmin(b);max=bmax(b);}
        if(!ray.box(min,max,distance)){i=node.skip;continue;}if(node.count==0){i++;continue;}
        for(int j=node.start;j<node.start+node.count;j+=3){
            const Vertex&a=v.vertices[v.indices[j]],&b=v.vertices[v.indices[j+1]],&c=v.vertices[v.indices[j+2]];
            float t;V n{};if(!triangle(ray,mixed?posed(v,a,s.pose):a.point,mixed?posed(v,b,s.pose):b.point,mixed?posed(v,c,s.pose):c.point,distance,t,n))continue;
            distance=t;hitNormal=mixed?n:normal(inv,n);found=true;
        }i=node.skip;
    }return found;
}
static int build(View&v,int start,int count,int&next){
    int slot=next++;Bounds bounds=v.bounds[v.order[start]];
    for(int i=start+1;i<start+count;i++)bounds=united(bounds,v.bounds[v.order[i]]);
    WorldNode n{bmin(bounds),bmax(bounds),start,count,0};
    if(count>4){
        V size=bounds.extents;int axis=size.x>size.y?(size.x>size.z?0:2):(size.y>size.z?1:2);
        float pivot=bounds.center[axis];int left=start,right=start+count-1;
        while(left<=right){if(v.bounds[v.order[left]].center[axis]<pivot){left++;continue;}std::swap(v.order[left],v.order[right--]);}
        int half=left-start;if(half==0||half==count)half=count/2;build(v,start,half,next);build(v,start+half,count-half,next);n.count=0;
    }n.skip=next;v.nodes[slot]=n;return slot;
}
extern "C" __declspec(dllexport) unsigned eeo_contact_abi(){return 0x10003;}
extern "C" __declspec(dllexport) unsigned eeo_contact_size(int type){
    const unsigned sizes[]={sizeof(V),sizeof(M),sizeof(Bounds),sizeof(Node),sizeof(Vertex),sizeof(Influence),sizeof(Envelope),sizeof(Surface),sizeof(Flight),sizeof(Result),sizeof(WorldNode),sizeof(View),sizeof(RayInput)};
    return type>=0&&type<13?sizes[type]:0;
}
extern "C" __declspec(dllexport) void eeo_contact_index(View*v,int count,int rebuild){
    if(count==0){v->nodes[0]={{},{},-1,0,1};return;}
    bool lifeChanged=false;
    for(int i=0;i<count;i++){
        const Surface&s=v->surfaces[i];
        bool wasLive=v->bounds[i].extents.x>=0;lifeChanged|=(s.live!=0)!=wasLive;
        if(s.live)v->bounds[i]=s.kind>=2?transformed(bmin(s.primitive),bmax(s.primitive),v->world[s.pose]):nodeBounds(*v,v->geometry[s.root],s.pose);
        else if(wasLive)v->bounds[i]={{},{-1,-1,-1}};
        if(rebuild)v->order[i]=i;
    }
    if(rebuild){int next=0;build(*v,0,count,next);}
    WorldNode&metadata=v->nodes[count*2-1];
    bool checkQuality=rebuild||lifeChanged||(++metadata.count%64==0);
    double cost=0;
    for(int i=v->nodes[0].skip-1;i>=0;i--){
        WorldNode&n=v->nodes[i];if(n.start<0)continue;
        if(!lifeChanged&&!rebuild&&n.min.x>n.max.x)continue;
        if(!rebuild){
            if(n.count>0){Bounds b=v->bounds[v->order[n.start]];for(int j=1;j<n.count;j++)b=united(b,v->bounds[v->order[n.start+j]]);n.min=bmin(b);n.max=bmax(b);}
            else{const WorldNode&left=v->nodes[i+1],&right=v->nodes[left.skip];
                if(left.min.x>left.max.x){n.min=right.min;n.max=right.max;}
                else if(right.min.x>right.max.x){n.min=left.min;n.max=left.max;}
                else{n.min=lo(left.min,right.min);n.max=hi(left.max,right.max);}}
        }
        if(checkQuality&&n.min.x<=n.max.x){V e=sub(n.max,n.min);cost+=static_cast<double>(e.x)*e.y+static_cast<double>(e.y)*e.z+static_cast<double>(e.z)*e.x;}
    }
    // The last reserved node is outside a binary tree's maximum 2*N-1 nodes.
    // Rebuild only after a measured bounds-quality regression, not every volley
    // or every 32 frames. Activation/deactivation simply refits existing leaves.
    float&baseline=metadata.min.x;
    if(rebuild){baseline=static_cast<float>(std::max(1e-12,cost));metadata.count=0;}
    else if(checkQuality&&cost>static_cast<double>(baseline)*2){eeo_contact_index(v,count,1);}
}
extern "C" __declspec(dllexport) void eeo_contact_query(const View*v,int first,int count){
    for(int fidx=first;fidx<first+count;fidx++){
        const Flight&f=v->flights[fidx];Result r{};r.surface=-1;if(!f.live){v->results[fidx]=r;continue;}
        r.start=f.parent<0?f.previous:point(v->world[f.parent],f.previous);r.end=point(v->world[f.pose],{});
        r.next=f.parent<0?r.end:point(inverted(v->world[f.parent]),r.end);V range=sub(r.end,f.origin);
        r.expired=std::sqrt(dot(range,range))/.05f>f.maximum?1:0;
        V delta=sub(r.end,r.start);float length=std::sqrt(dot(delta,delta));r.distance=length;
        if(r.expired||!std::isfinite(length)||length<=0){v->results[fidx]=r;continue;}
        Ray ray(r.start,{delta.x/length,delta.y/length,delta.z/length});int end=v->nodes[0].skip;
        for(int i=0;i<end;){
            const WorldNode&n=v->nodes[i];if(n.start<0||!ray.box(n.min,n.max,r.distance)){i=n.skip;continue;}if(n.count==0){i++;continue;}
            for(int j=n.start;j<n.start+n.count;j++){
                int id=v->order[j];const Surface&s=v->surfaces[id];if(!s.live||s.body==f.body||s.faction==f.faction||(static_cast<unsigned>(f.mask)&(1u<<s.layer))==0)continue;
                float distance;V hitNormal{};if(!intersect(*v,s,ray,r.distance,distance,hitNormal))continue;
                if(r.surface>=0&&distance==r.distance&&id<r.surface)continue;
                r.surface=id;r.distance=distance;r.normal=hitNormal;r.generation=s.generation;
            }i=n.skip;
        }
        r.point=add(r.start,mul(ray.d,r.distance));v->results[fidx]=r;
    }
}
static int raycastProxies(const View*v,const RayInput*input,int capacity,bool nearest,int faction){
    float length=std::sqrt(dot(input->direction,input->direction));
    if(!std::isfinite(length)||length<=0||input->maximum<0||std::isnan(input->maximum))return 0;
    Ray ray(input->origin,{input->direction.x/length,input->direction.y/length,input->direction.z/length});
    int hits=0,end=v->nodes[0].skip;float maximum=input->maximum;
    for(int i=0;i<end;){
        const WorldNode&n=v->nodes[i];
        if(n.start<0||!ray.box(n.min,n.max,maximum)){i=n.skip;continue;}
        if(n.count==0){i++;continue;}
        for(int j=n.start;j<n.start+n.count;j++){
            int id=v->order[j];const Surface&s=v->surfaces[id];
            if(!s.live||(nearest&&s.faction==faction)||(static_cast<unsigned>(input->mask)&(1u<<s.layer))==0)continue;
            M inv=inverted(v->world[s.pose]);V o=point(inv,ray.o),d=vector(inv,ray.d);
            V min=bmin(s.primitive),max=bmax(s.primitive),offset=sub(o,s.primitive.center);
            bool inside=s.kind==3?dot(offset,offset)<=s.primitive.extents.x*s.primitive.extents.x:
                o.x>=min.x&&o.x<=max.x&&o.y>=min.y&&o.y<=max.y&&o.z>=min.z&&o.z<=max.z;
            if(inside)continue;
            float distance;V face{};if(!primitive(s,o,d,maximum,distance,face))continue;
            if((!nearest&&hits>=capacity)||(nearest&&capacity<1))return -1;
            Result r{};r.surface=id;r.distance=distance;r.point=add(ray.o,mul(ray.d,distance));
            r.normal=normal(inv,face);r.generation=s.generation;
            if(nearest){v->results[0]=r;hits=1;maximum=distance;}else v->results[hits++]=r;
        }i=n.skip;
    }return hits;
}

extern "C" __declspec(dllexport) int eeo_contact_raycast(const View*v,const RayInput*input,int capacity){return raycastProxies(v,input,capacity,false,0);}
extern "C" __declspec(dllexport) int eeo_contact_nearest(const View*v,const RayInput*input,int faction){return raycastProxies(v,input,1,true,faction);}

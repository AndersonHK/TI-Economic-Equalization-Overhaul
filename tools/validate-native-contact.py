"""Independent ABI/analytic fixtures for the packaged numerical kernel (no Unity host)."""
import ctypes as c
import math
import random
import sys

class V(c.Structure):
    _fields_ = [(name, c.c_float) for name in ('x', 'y', 'z')]
class M(c.Structure):
    _fields_ = [('v', c.c_float * 16)]
class B(c.Structure):
    _fields_ = [('center', V), ('extents', V)]
class N(c.Structure):
    _fields_ = [('min', V), ('max', V)] + [(n, c.c_int) for n in ('start','count','skip','envelopeStart','envelopeCount')]
class Vertex(c.Structure):
    _fields_ = [('point', V), ('start',c.c_int), ('count',c.c_int)]
class Influence(c.Structure):
    _fields_ = [('point', V), ('weight',c.c_float), ('bone',c.c_int)]
class Envelope(c.Structure):
    _fields_ = [('min',V), ('max',V), ('bone',c.c_int)]
class Surface(c.Structure):
    _fields_ = [(n,c.c_int) for n in ('root','pose','body','faction','layer','kind','live','generation')] + [('bounds',B)]
class Flight(c.Structure):
    _fields_ = [('previous',V),('origin',V)] + [(n,c.c_int) for n in ('pose','parent','body','faction','mask','live')] + [('maximum',c.c_float)]
class Result(c.Structure):
    _fields_ = [(n,V) for n in ('start','end','next','point','normal')] + [('distance',c.c_float)] + [(n,c.c_int) for n in ('surface','expired','generation')]
class W(c.Structure):
    _fields_ = [('min',V),('max',V)] + [(n,c.c_int) for n in ('start','count','skip')]
class RayInput(c.Structure):
    _fields_=[("origin",V),("direction",V),("maximum",c.c_float),("mask",c.c_int)]
class View(c.Structure):
    _fields_ = [(n,c.c_void_p) for n in ('geometry','vertices','indices','influences','envelopes','surfaces','world','inverse','flights','results','bounds','order','nodes')]

dll=c.CDLL(sys.argv[1])
dll.eeo_contact_abi.restype=c.c_uint
dll.eeo_contact_size.argtypes=[c.c_int]
dll.eeo_contact_size.restype=c.c_uint
dll.eeo_contact_index.argtypes=[c.POINTER(View),c.c_int,c.c_int]
dll.eeo_contact_query.argtypes=[c.POINTER(View),c.c_int,c.c_int]
dll.eeo_contact_raycast.argtypes=[c.POINTER(View),c.POINTER(RayInput),c.c_int]
dll.eeo_contact_raycast.restype=c.c_int
dll.eeo_contact_nearest.argtypes=[c.POINTER(View),c.POINTER(RayInput),c.c_int]
dll.eeo_contact_nearest.restype=c.c_int
assert dll.eeo_contact_abi()==0x10003
for i,typ in enumerate((V,M,B,N,Vertex,Influence,Envelope,Surface,Flight,Result,W,View,RayInput)):
    assert dll.eeo_contact_size(i)==c.sizeof(typ),(typ.__name__,dll.eeo_contact_size(i),c.sizeof(typ))

def matrix(angle=0,scale=(1,1,1),translation=(0,0,0)):
    co,si=math.cos(angle),math.sin(angle)
    return M((c.c_float*16)(co*scale[0],si*scale[0],0,0,-si*scale[1],co*scale[1],0,0,0,0,scale[2],0,*translation,1))
def point(m,p):
    return V(*(sum(m.v[r+4*k]*p[k] for k in range(3))+m.v[r+12] for r in range(3)))
def distance(a,b):
    return math.sqrt(sum((getattr(a,k)-getattr(b,k))**2 for k in ('x','y','z')))
def f32(x):
    return c.c_float(x).value
def sphere_reference(m,start,end):
    # The ray and inverse mapping are single-precision inputs to the collision
    # contract. Derive those rounded inputs before applying an independent
    # quadratic equation; ideal pre-rounding y/z gives a false oracle for a
    # translated ellipsoid with a 4,000:1 axis ratio near a tangent.
    delta=[f32(getattr(end,k)-getattr(start,k)) for k in ('x','y','z')]
    length=f32(math.sqrt(f32(f32(f32(delta[0]*delta[0])+f32(delta[1]*delta[1]))+f32(delta[2]*delta[2]))))
    direction=[f32(x/length) for x in delta]
    a,b,d,e=m.v[0],m.v[4],m.v[1],m.v[5];det=a*e-b*d
    rows=[[f32(e/det),f32(-b/det),0],[f32(-d/det),f32(a/det),0],[0,0,f32(1/m.v[10])]]
    def dot32(row,p):return f32(f32(f32(row[0]*p[0])+f32(row[1]*p[1]))+f32(row[2]*p[2]))
    translation=[m.v[12],m.v[13],m.v[14]]
    origin=[f32(dot32(row,[start.x,start.y,start.z])-dot32(row,translation)) for row in rows]
    local_direction=[dot32(row,direction) for row in rows]
    aa=sum(x*x for x in local_direction);bb=2*sum(x*y for x,y in zip(origin,local_direction));cc=sum(x*x for x in origin)-.25
    discriminant=bb*bb-4*aa*cc
    if discriminant<0:return None
    t=(-bb-math.sqrt(discriminant))/(2*aa)
    if t<0:t=(-bb+math.sqrt(discriminant))/(2*aa)
    if not 0<=t<=length:return None
    return V(*(getattr(start,k)+direction[j]*t for j,k in enumerate(('x','y','z'))))

geometry=(N*1)(N(V(0,-1,-1),V(0,1,1),0,3,1,0,0))
vertices=(Vertex*3)(Vertex(V(0,-1,-1),0,0),Vertex(V(0,1,-1),0,0),Vertex(V(0,0,1),0,0))
indices=(c.c_int*3)(0,1,2)
influences=(Influence*6)()
envelopes=(Envelope*2)(Envelope(V(0,-1,-1),V(0,1,1),0),Envelope(V(0,-1,-1),V(0,1,1),2))
surfaces=(Surface*16)()
world=(M*18)()
inverse=(M*18)()  # Intentionally zero: native candidate inverses must derive from current world poses.
flights=(Flight*1)()
results=(Result*1)()
bounds=(B*16)()
order=(c.c_int*16)()
nodes=(W*32)()
arrays=(geometry,vertices,indices,influences,envelopes,surfaces,world,inverse,flights,results,bounds,order,nodes)
view=View(*(c.cast(a,c.c_void_p).value for a in arrays))
checks=13
rng=random.Random(3417)
for kind in (0,1,2,3):
    for case in range(350):
        angle=rng.uniform(-math.pi,math.pi)
        scale=tuple(rng.choice((-.0005,.0005,-.5,.5,1,2)) for _ in range(3))
        world[0]=matrix(angle,scale,(10,-2,3))
        world[2]=matrix(angle,scale,tuple(getattr(point(world[0],(4,0,0)),k) for k in ('x','y','z')))
        y=rng.uniform(-1.4,1.4);z=rng.uniform(-.8,.8)
        surfaces[0]=Surface(0,0,42,2,0,kind,1,91,B(V(),V(.5,.5,.5)))
        geometry[0].envelopeCount=2 if kind==1 else 0
        for j in range(3):
            vertices[j].start=j*2;vertices[j].count=2 if kind==1 else 0
            influences[j*2]=Influence(vertices[j].point,.7,0)
            influences[j*2+1]=Influence(vertices[j].point,.3,2)
        start=point(world[0],(-10,y,z));end=point(world[0],(10,y,z))
        world[1]=matrix(translation=(end.x,end.y,end.z))
        flights[0]=Flight(start,start,1,-1,99,1,1,1,100000)
        expected=abs(y)<(1-z)/2 if kind in (0,1) else (abs(y)<.5 and abs(z)<.5) if kind==2 else y*y+z*z<.25
        local_x=0 if kind==0 else 1.2 if kind==1 else -.5 if kind==2 else -math.sqrt(max(0,.25-y*y-z*z))
        exact=point(world[0],(local_x,y,z))
        if kind==3:
            reference=sphere_reference(world[0],start,end)
            expected=reference is not None
            if expected:exact=reference
        # Avoid floating-point boundary ambiguity in the tiny-scale fixtures.
        if kind==3 and abs(y*y+z*z-.25)<.002:continue
        if kind in (0,1) and abs(abs(y)-(1-z)/2)<.002:continue
        dll.eeo_contact_index(c.byref(view),1,1)
        dll.eeo_contact_query(c.byref(view),0,1)
        got=results[0];checks+=1
        assert (got.surface>=0)==expected,(kind,case,y,z,scale,got.surface)
        if expected:
            assert distance(got.point,exact)<.00005,(kind,case,distance(got.point,exact))
            assert got.generation==91
        for reason in range(5):
            old=Surface.from_buffer_copy(surfaces[0]);oldflight=Flight.from_buffer_copy(flights[0])
            if reason==0:surfaces[0].live=0
            elif reason==1:surfaces[0].faction=1
            elif reason==2:surfaces[0].body=99
            elif reason==3:flights[0].mask=0
            else:flights[0].maximum=0
            dll.eeo_contact_index(c.byref(view),1,1);dll.eeo_contact_query(c.byref(view),0,1)
            assert results[0].surface==-1,(kind,case,reason);checks+=1
            surfaces[0]=old;flights[0]=oldflight

# Branched world index, nearest result and moving-body refit, independently
# predicted from planes perpendicular to the flight path.
geometry[0].envelopeCount=0
for v in vertices:v.count=0
for i in range(16):surfaces[i]=Surface(0,i,42+i,2,0,0,1,i,B())
start=V(0,0,0);world[16]=matrix(translation=(100,0,0));flights[0]=Flight(start,start,16,-1,99,1,1,1,100000)
for step in range(80):
    positions=list(range(2,18));rng.shuffle(positions)
    for i,x in enumerate(positions):world[i]=matrix(translation=(x,0,0))
    dll.eeo_contact_index(c.byref(view),16,int(step%11==0))
    dll.eeo_contact_query(c.byref(view),0,1)
    assert abs(results[0].point.x-2)<1e-5 and results[0].surface==positions.index(2);checks+=1
# Strict endpoint before the first plane.
world[16]=matrix(translation=(1.999,0,0));dll.eeo_contact_query(c.byref(view),0,1)
assert results[0].surface==-1;checks+=1
# Reserved inactive leaves become live (and become inactive again) without a
# topology rebuild. This is the projectile reuse/visibility-change path.
world[16]=matrix(translation=(100,0,0))
for s in surfaces:s.live=0
dll.eeo_contact_index(c.byref(view),16,1)
for i in range(16):
    surfaces[i].live=1
    dll.eeo_contact_index(c.byref(view),16,0);dll.eeo_contact_query(c.byref(view),0,1)
    assert results[0].surface==i;checks+=1
    surfaces[i].live=0
    dll.eeo_contact_index(c.byref(view),16,0);dll.eeo_contact_query(c.byref(view),0,1)
    assert results[0].surface==-1;checks+=1
# Parent-relative history must follow a rebased scene, including mirrored roots.
surfaces[0]=Surface(0,0,42,2,0,0,1,31,B())
for k,offset in enumerate((100,200,300)):
    world[0]=world[17]=matrix(scale=(-2,.5,1),translation=(offset,0,0))
    end=point(world[17],(10,0,0));world[16]=matrix(translation=(end.x,end.y,end.z))
    flights[0]=Flight(V(-10,0,0),V(),16,17,99,1,1,1,100000)
    dll.eeo_contact_index(c.byref(view),1,int(k==0));dll.eeo_contact_query(c.byref(view),0,1)
    assert results[0].surface==0 and distance(results[0].point,V(offset,0,0))<1e-5
    assert distance(results[0].next,V(10,0,0))<1e-5;checks+=2
# Compatibility rays: all fitted candidates, no friendly filtering, and native
# inside-origin rejection. This is distinct from travelled ballistic semantics.
proxy_results=(Result*16)();view.results=c.cast(proxy_results,c.c_void_p).value
for i in range(16):
    surfaces[i]=Surface(0,i,42+i,1,0,2,1,i,B(V(),V(.5,.5,.5)))
    world[i]=matrix(translation=(i*2+2,0,0))
dll.eeo_contact_index(c.byref(view),16,1)
for stop in (0,1.499,1.5,3.5,17.5,100):
    ray=RayInput(V(),V(7,0,0),stop,1)
    count=dll.eeo_contact_raycast(c.byref(view),c.byref(ray),16)
    expected=[i for i in range(16) if i*2+1.5<=stop]
    assert sorted(proxy_results[i].surface for i in range(count))==expected;checks+=1
ray=RayInput(V(2,0,0),V(1,0,0),100,1)
count=dll.eeo_contact_raycast(c.byref(view),c.byref(ray),16)
assert count==15 and all(proxy_results[i].surface!=0 for i in range(count));checks+=1
ray=RayInput(V(),V(1,0,0),100,0)
assert dll.eeo_contact_raycast(c.byref(view),c.byref(ray),16)==0;checks+=1
ray=RayInput(V(),V(1,0,0),100,1)
assert dll.eeo_contact_raycast(c.byref(view),c.byref(ray),1)==-1;checks+=1
assert dll.eeo_contact_nearest(c.byref(view),c.byref(ray),1)==0;checks+=1
surfaces[7].faction=2
assert dll.eeo_contact_nearest(c.byref(view),c.byref(ray),1)==1 and proxy_results[0].surface==7;checks+=1
surfaces[5].faction=2
assert dll.eeo_contact_nearest(c.byref(view),c.byref(ray),1)==1 and proxy_results[0].surface==5;checks+=1
surfaces[5].live=0
assert dll.eeo_contact_nearest(c.byref(view),c.byref(ray),1)==1 and proxy_results[0].surface==7;checks+=1
surfaces[5].live=1;ray.origin=V(12,0,0)
assert dll.eeo_contact_nearest(c.byref(view),c.byref(ray),1)==1 and proxy_results[0].surface==7;checks+=1
print(f'PASS: {checks} native ABI, analytic rigid/blended/primitive, tiny/mirrored/nonuniform transforms, filtering, generation emission, nearest, refit and strict-segment checks.')

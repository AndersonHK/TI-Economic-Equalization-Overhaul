using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace TIEconomyMod.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ContactNativeView
    {
        internal IntPtr Geometry,Vertices,Indices,Influences,Envelopes,Surfaces,World,Inverse,Flights,Results,Bounds,Order,Nodes;
    }
    internal static unsafe class NativeContact
    {
        private static IntPtr library;
        private static bool validated;
        [DllImport("kernel32",CharSet=CharSet.Unicode,SetLastError=true)]
        private static extern IntPtr LoadLibraryEx(string file,IntPtr reserved,uint flags);
        [DllImport("TIEconomyContact.dll",CallingConvention=CallingConvention.Cdecl)]
        private static extern uint eeo_contact_abi();
        [DllImport("TIEconomyContact.dll",CallingConvention=CallingConvention.Cdecl)]
        private static extern uint eeo_contact_size(int type);
        [DllImport("TIEconomyContact.dll",CallingConvention=CallingConvention.Cdecl)]
        private static extern void eeo_contact_index(ref ContactNativeView view,int count,int rebuild);
        [DllImport("TIEconomyContact.dll",CallingConvention=CallingConvention.Cdecl)]
        private static extern void eeo_contact_query(ref ContactNativeView view,int first,int count);
        [DllImport("TIEconomyContact.dll",CallingConvention=CallingConvention.Cdecl)]
        private static extern int eeo_contact_raycast(ref ContactNativeView view,ref ContactRay ray,int capacity);
        [DllImport("TIEconomyContact.dll",CallingConvention=CallingConvention.Cdecl)]
        private static extern int eeo_contact_nearest(ref ContactNativeView view,ref ContactRay ray,int faction);
        internal static void Load(string modPath)
        {
            if(validated)return;
            if(IntPtr.Size!=8)throw new PlatformNotSupportedException("The contact kernel requires the supported 64-bit player.");
            string path=Path.GetFullPath(Path.Combine(modPath,"Assembly","TIEconomyContact.dll"));
            if(library==IntPtr.Zero)library=LoadLibraryEx(path,IntPtr.Zero,0x1100);
            if(library==IntPtr.Zero)throw new InvalidOperationException("Native contact kernel load failed: "+Marshal.GetLastWin32Error());
            if(eeo_contact_abi()!=0x10003)throw new InvalidOperationException("Native contact ABI version mismatch; restart with a matched package.");
            Type[] types={typeof(Vector3),typeof(Matrix4x4),typeof(Bounds),typeof(ContactNode),typeof(ContactVertex),typeof(ContactInfluence),typeof(ContactEnvelope),typeof(ContactSurface),typeof(ContactFlight),typeof(ContactResult),typeof(ContactWorldNode),typeof(ContactNativeView),typeof(ContactRay)};
            for(int i=0;i<types.Length;i++)if(eeo_contact_size(i)!=Marshal.SizeOf(types[i]))throw new InvalidOperationException("Native contact layout mismatch: "+types[i].Name);
            if(Marshal.OffsetOf(typeof(Matrix4x4),"m03").ToInt32()!=48||Marshal.OffsetOf(typeof(Matrix4x4),"m10").ToInt32()!=4)
                throw new InvalidOperationException("Native contact matrix layout changed.");
            validated=true;
            Main.Log("Native contact kernel loaded: ABI 0x10003, thirteen record layouts verified.");
        }
        private static IntPtr Read<T>(NativeArray<T> data) where T:struct
        {return (IntPtr)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(data);}
        private static IntPtr Write<T>(NativeArray<T> data) where T:struct
        {return (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(data);}
        internal static void Index(ContactIndexJob job)
        {
            ContactNativeView view=new ContactNativeView{Geometry=Read(job.Geometry),Envelopes=Read(job.Envelopes),
                Surfaces=Read(job.Surfaces),World=Read(job.World),Bounds=Write(job.Bounds),Order=Write(job.Order),Nodes=Write(job.Nodes)};
            eeo_contact_index(ref view,job.Count,job.Rebuild?1:0);
        }
        internal static void Query(ContactFlightJob job,int first,int count)
        {
            ContactNativeView view=new ContactNativeView{Geometry=Read(job.Geometry),Vertices=Read(job.Vertices),Indices=Read(job.Indices),
                Influences=Read(job.Influences),Envelopes=Read(job.Envelopes),Surfaces=Read(job.Surfaces),World=Read(job.World),Inverse=Read(job.Inverse),
                Flights=Read(job.Flights),Results=Write(job.Results),Order=Read(job.Order),Nodes=Read(job.Nodes)};
            eeo_contact_query(ref view,first,count);
        }
        internal static int RaycastProxies(NativeArray<ContactSurface> proxies,NativeArray<Matrix4x4> poses,
            NativeArray<ContactWorldNode> nodes,NativeArray<int> order,NativeArray<ContactResult> results,ContactRay ray,int faction=-1)
        {
            ContactNativeView view=new ContactNativeView{Surfaces=Read(proxies),World=Read(poses),Nodes=Read(nodes),Order=Read(order),Results=Write(results)};
            return faction<0?eeo_contact_raycast(ref view,ref ray,results.Length):eeo_contact_nearest(ref view,ref ray,faction);
        }
    }
}

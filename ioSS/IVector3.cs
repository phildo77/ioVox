using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY
using UnityEngine;
#endif

namespace ioSS
{
    public class IVector3
    {

        public int x, y, z;

        public static readonly IVector3 Zero = new IVector3(0,0,0);

        //Axis Directions
        public static readonly IVector3 Xp = new IVector3(1, 0, 0);
        public static readonly IVector3 Xn = new IVector3(-1, 0, 0);
        public static readonly IVector3 Yp = new IVector3(0, 1, 0);
        public static readonly IVector3 Yn = new IVector3(0, -1, 0);
        public static readonly IVector3 Zp = new IVector3(0, 0, 1);
        public static readonly IVector3 Zn = new IVector3(0, 0, -1);
        public static readonly List<IVector3> Dirs3D = new List<IVector3>() { Xp, Xn, Yp, Yn, Zp, Zn };
        //public static readonly List<IVector3> DirsXY = new List<IVector3>() { Yp, Xp, Yn, Xn };
        //public static readonly List<IVector3> DirsXZ = new List<IVector3>() { Zp, Xp, Zn, Xn };
        //public static readonly List<IVector3> DirsYZ = new List<IVector3>() { Yp, Zp, Yn, Zn };

        public static List<IVector3> GetPlaneAxes(IVector3 _norm)
        {
            if (_norm == Xp) return new List<IVector3>() { Zp, Yp };
            if (_norm == Xn) return new List<IVector3>() { Zn, Yp };
            if (_norm == Yp) return new List<IVector3>() { Xp, Zp };
            if (_norm == Yn) return new List<IVector3>() { Xn, Zp };
            if (_norm == Zp) return new List<IVector3>() { Xn, Yp };
            if (_norm == Zn) return new List<IVector3>() { Xp, Yp };

            return null;  //Return null on invalid direction
        }

        public static List<FVector3> GetPlaneAxes(FVector3 _norm)
        {
            if (_norm == (FVector3)Xp) return new List<FVector3>() { (FVector3)Zp, (FVector3)Yp };
            if (_norm == (FVector3)Xn) return new List<FVector3>() { (FVector3)Zn, (FVector3)Yp };
            if (_norm == (FVector3)Yp) return new List<FVector3>() { (FVector3)Xp, (FVector3)Zp };
            if (_norm == (FVector3)Yn) return new List<FVector3>() { (FVector3)Xn, (FVector3)Zp };
            if (_norm == (FVector3)Zp) return new List<FVector3>() { (FVector3)Xn, (FVector3)Yp };
            if (_norm == (FVector3)Zn) return new List<FVector3>() { (FVector3)Xp, (FVector3)Yp };

            return null;  //Return null on invalid direction
        }

        //Neighbors
        public IVector3 Xnext() { return new IVector3(x + 1, y, z); }
        public IVector3 Xprev() { return new IVector3(x - 1, y, z); }
        public IVector3 Ynext() { return new IVector3(x, y + 1, z); }
        public IVector3 Yprev() { return new IVector3(x, y - 1, z); }
        public IVector3 Znext() { return new IVector3(x, y, z + 1); }
        public IVector3 Zprev() { return new IVector3(x, y, z - 1); }
        public IVector3[] Neighbors()
        {
            return new IVector3[6] 
            {
                new IVector3(x + 1, y, z),
                new IVector3(x - 1, y, z),
                new IVector3(x, y + 1, z),
                new IVector3(x, y - 1, z),
                new IVector3(x, y, z + 1),
                new IVector3(x, y, z - 1)
            };
        }


        public IVector3(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public IVector3(IVector3 _coord)
        {
            x = _coord.x;
            y = _coord.y;
            z = _coord.z;
        }


        public override int GetHashCode()
        {
            return x ^ y ^ z;
        }

        public bool Equals(IVector3 vector)
        {
            // Return true if the fields match:
            return (x == vector.x) && (y == vector.y) && (z == vector.z);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }


        public static bool operator ==(IVector3 a, IVector3 b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(IVector3 a, IVector3 b)
        {
            return !(a == b);
        }

        public static IVector3 operator +(IVector3 a, IVector3 b)
        {
            return new IVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static IVector3 operator -(IVector3 a, IVector3 b)
        {
            return new IVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static IVector3 operator -(IVector3 a)
        {
            return new IVector3(-a.x, -a.y, -a.z);
        }

        public static IVector3 operator *(IVector3 a, int b)
        {
            return new IVector3(a.x * b, a.y * b, a.z * b);
        }

        public static IVector3 operator *(int a, IVector3 b)
        {
            return new IVector3(a * b.x, a * b.y, a * b.z);
        }

        public override string ToString()
        {
            return x + ", " + y + ", " + z;
        }

        public static implicit operator FVector3(IVector3 _a)
        {
            return new FVector3(_a.x, _a.y, _a.z);
        }

#if UNITY
        public static explicit operator Vector3(IVector3 _a)
        {
            return new Vector3(_a.x, _a.y, _a.z);
        }
#endif

        /*public bool IsValidDirection(IVector3 _dir)
        {
            if (Dirs3D.Contains(_dir))
            {
                return true;
            }
            else
            {
                return false;
            }

        }*/

        public static bool IsValidDirection(IVector3 _dir)
        {
            if (Dirs3D.Contains(_dir))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        
        

    }




    
}
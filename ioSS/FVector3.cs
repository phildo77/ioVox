
using System;

#if UNITY
using UnityEngine;
#endif


namespace ioSS
{
    public class FVector3
    {

        public float x, y, z;

        public FVector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public FVector3(FVector3 _a)
        {
            x = _a.x;
            y = _a.y;
            z = _a.z;
        }

        public FVector3()
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }
        
        /*public override int GetHashCode()
        {

            //return x ^ y ^ z;
            //TODO
        }*/

        public bool Equals(FVector3 vector)
        {
            // Return true if the fields match:
            return (x == vector.x) && (y == vector.y) && (z == vector.z);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(FVector3 a, FVector3 b)
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

        public static bool operator !=(FVector3 a, FVector3 b)
        {
            return !(a == b);
        }

        public static FVector3 operator +(FVector3 a, FVector3 b)
        {
            return new FVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static FVector3 operator -(FVector3 a, FVector3 b)
        {
            return new FVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static FVector3 operator -(FVector3 a)
        {
            return new FVector3(-a.x, -a.y, -a.z);
        }

        public static FVector3 operator *(FVector3 b, float a)
        {
            return new FVector3(a * b.x, a * b.y, a * b.z);
        }

        public static FVector3 operator /(FVector3 b, float a)
        {
            return new FVector3(b.x / a, b.y / a, b.z / a);
        }

        public static explicit operator IVector3(FVector3 _a)
        {
            return new IVector3((int)_a.x, (int)_a.y, (int)_a.z);
        }

#if UNITY
        public static explicit operator Vector3(FVector3 _a)
        {
            return new Vector3(_a.x, _a.y, _a.z);
        }
#endif

        public override string ToString()
        {
            return x + ", " + y + ", " + z;
        }

        

        /// <summary>
        /// Returns the world coordinate of a vector moved by a local surface x,y,z.  Surface is defined by world origin and world facing.
        /// </summary>
        /// <param name="_toMoveW">world coordinate of vector to move</param>
        /// <param name="_origin">World coordinate of surface origin</param>
        /// <param name="_facingW">Facing directino of surface</param>
        /// <param name="_MoveBy">Local move by x,y,z</param>
        /// <returns></returns>
        /*public static FVector3 LocalMove(FVector3 _toMoveW, FVector3 _MoveByL, IVector3 _originW, IVector3 _facingW)
        {

            if (!IVector3.IsValidDirection(_facingW))
            {
                throw new ArgumentException("Localize Vector- Direction " + _facingW + " not a cardinal direction.");
            }
            FVector3 localVec = LocalizeVector(_toMoveW, _originW, _facingW);
            
            if (_facingW == IVector3.Xp)
            {
                //return new FVector3(localVec.z, localVec.y, localVec.x);
                return _originW + new FVector3(localVec.z + _MoveByL.z, localVec.y + _MoveByL.y, localVec.x + _MoveByL.x);
            }
            else if (_facingW == IVector3.Xn)
            {
                return _originW + new FVector3(localVec.z - _MoveByL.z, localVec.y + _MoveByL.y, localVec.x - _MoveByL.x);
            }
            else if (_facingW == IVector3.Yp)
            {
                return _originW + new FVector3(localVec.x + _MoveByL.x, localVec.z + _MoveByL.z, localVec.y + _MoveByL.y);
            }
            else if (_facingW == IVector3.Yn)
            {
                return _originW + new FVector3(localVec.x - _MoveByL.x, localVec.z + _MoveByL.z, localVec.y - _MoveByL.y);
            }
            else if (_facingW == IVector3.Zn)
            {
                return _originW + new FVector3(localVec.x + _MoveByL.x, localVec.y + _MoveByL.y, localVec.z - _MoveByL.z);
            }
            else if (_facingW == IVector3.Zp)
            {
                return _originW + new FVector3(localVec.x - _MoveByL.x, localVec.y + _MoveByL.y, localVec.z + _MoveByL.z);
            }
            else
            {
                ioDebug.Log("LocalMove - Invalid facing direction: " + _facingW);
                return null;
            }
        }*/

        

        

    }




    
}
using System;
using System.Collections.Generic;
using System.Xml;
using CDataArray.ioVector;
#if UNITY
using UnityEngine;
#endif

namespace ioSS.ioVox
{

    public struct ioMesh
    {
        public readonly List<FVector3> Verts;
        public readonly List<float[]> UVs;
        public readonly List<int> Tris;
        public readonly List<FVector3> Normals;

        public ioMesh(List<FVector3> _verts, List<float[]> _uvs, List<int> _tris, List<FVector3> _normals)
        {
            Verts = _verts;
            UVs = _uvs;
            Tris = _tris;
            Normals = _normals;
        }

#if UNITY
        public static explicit operator Mesh(ioMesh _a)
        {
            Mesh uMesh = new Mesh();

            List<Vector3> uVerts = new List<Vector3>();
            foreach (FVector3 vert in _a.Verts)
            {
                uVerts.Add((Vector3)vert);
            }

            List<Vector2> uUVs = new List<Vector2>();
            foreach (float[] uv in _a.UVs)
            {
                uUVs.Add(new Vector2(uv[0], uv[1]));
            }

            List<Vector3> uNorms = new List<Vector3>();
            foreach (FVector3 norm in _a.Normals)
            {
                uNorms.Add((Vector3)norm);
            }

            uMesh.vertices = uVerts.ToArray();
            uMesh.uv = uUVs.ToArray();
            uMesh.normals = uNorms.ToArray();
            uMesh.triangles = _a.Tris.ToArray();

            return uMesh;

        }
#endif
    }

    public struct Voxel
    {
        //Data, IsVisible, FaceType, IsTransparent
        public readonly ushort id;
        public readonly string name;
        public readonly string desc;
        public readonly bool isVisible;
        public readonly bool isTransp;
        public readonly FaceType faceType;

        public Voxel(ushort _id, string _name, string _desc, bool _visible, bool _trasparent, FaceType _faceType)
        {
            id = _id;
            name = _name;
            desc = _desc;
            isVisible = _visible;
            isTransp = _trasparent;
            faceType = _faceType;
        }
    }

    public static class VoxData
    {
        private static Dictionary<ushort, Voxel> m_DataTable = new Dictionary<ushort, Voxel>();

        //private static readonly string TAG_TYPES = "Types";
        private static readonly string TAG_TYPE = "Type";
        private static readonly string ATR_ID = "id";
        private static readonly string ATR_NAME = "name";
        private static readonly string ATR_DESC = "desc";
        private static readonly string ATR_VISIBLE = "isvisible";
        private static readonly string ATR_TRANSP = "istransp";
        private static readonly string ATR_FACETYPE = "facetype";

        public static string GetName(ushort _id)
        {
            return m_DataTable[_id].name;
        }

        public static string GetDescription(ushort _id)
        {
            return m_DataTable[_id].desc;
        }

        public static bool IsVisible(ushort _id)
        {
            return m_DataTable[_id].isVisible;
        }

        public static bool IsTransparent(ushort _id)
        {
            return m_DataTable[_id].isTransp;
        }

        public static FaceType GetFaceType(ushort _id)
        {
            return m_DataTable[_id].faceType;
        }

        public static Voxel? GetInfo(ushort _id)
        {
            if (IsValidID(_id))
            {
                return m_DataTable[_id];
            }
            else
            {
                ioDebug.Log("Error accessing material data: Key '" + _id + "' does not exist in material data table.  Returning null.");
                return null;
            }
        }

        public static bool IsValidID(ushort _id)
        {
            return ((_id < 0) || !m_DataTable.ContainsKey(_id)) ? false : true;
        }

        public static void DebugMaterialTableToConsole()
        {
            ioDebug.Log("Material Data Table-----------", true, ConsoleColor.Blue);
            ioDebug.Log("ID, Vis, Transp, Name, Desc\n", true, ConsoleColor.Blue);
            ConsoleColor fg = ConsoleColor.White;
            ConsoleColor bg = ConsoleColor.DarkGray;
            foreach (KeyValuePair<ushort, Voxel> Data in m_DataTable)
            {
                ioDebug.Log(Data.Key + ", " + Data.Value.isVisible + ", " + Data.Value.isTransp + ", " + Data.Value.name + ", " + Data.Value.desc, true, fg, bg);
                fg = (fg == ConsoleColor.White) ? fg = ConsoleColor.Gray : fg = ConsoleColor.White;
                bg = (bg == ConsoleColor.Black) ? bg = ConsoleColor.DarkGray : bg = ConsoleColor.Black;
            }
        }

        /// <summary>
        /// Loads material data from XML file into the material data table [m_DataTable].  Will delete all existing data if it is not empty.
        /// </summary>
        /// <param name="_path">Path to XML file containing the material data</param>
        public static void LoadDataFromFile(string _path)
        {

            if (!System.IO.File.Exists(_path))
            {
                ioDebug.Log("Loading material XML file '" + _path + "' does not exist.");
                return;
            }

            ioDebug.Log("Loading " + _path + " into material table.");


            try
            {
                XmlReader xmlReader = XmlReader.Create(_path);

                if (xmlReader.EOF)
                {
                    ioDebug.Log("XML file is empty or corrupt/invalid.");
                    return;
                }

                ushort count = 0;

                while (xmlReader.Read())
                {
                    if (xmlReader.Name.Equals(TAG_TYPE) && (xmlReader.NodeType == XmlNodeType.Element))
                    {

                        ushort id = ushort.Parse(xmlReader.GetAttribute(ATR_ID));
                        string name = xmlReader.GetAttribute(ATR_NAME).Trim();
                        string desc = xmlReader.GetAttribute(ATR_DESC).Trim();
                        string isVisibleStr = xmlReader.GetAttribute(ATR_VISIBLE).ToUpper().Trim();
                        string isTranspStr = xmlReader.GetAttribute(ATR_TRANSP).ToUpper().Trim();
                        string faceTypeStr = xmlReader.GetAttribute(ATR_FACETYPE).ToUpper().Trim();

                        bool isVisible = true;
                        bool isTransp = false;

                        //Verify values are valid
                        if (!Const.IsValidBool(isVisibleStr))
                        {
                            ioDebug.Log("Error while loading material data from XML:\n   Invalid bool string '" + isVisibleStr + "' for 'IsVisible' at element '" + count + "'. Defaulting TRUE.");

                        }
                        else
                        {
                            isVisible = Const.StringToBool(isVisibleStr);
                        }

                        if (!Const.IsValidBool(isTranspStr))
                        {
                            ioDebug.Log("Error while loading material data from XML:\n   Invalid bool string '" + isTranspStr + "' for 'IsTransp' at element '" + count + "'. Defaulting FALSE.");

                        }
                        else
                        {
                            isTransp = Const.StringToBool(isTranspStr);
                        }

                        FaceType faceType = StringToFaceType(faceTypeStr);

                        Voxel data = new Voxel( id,
                                                name,
                                                desc,
                                                isVisible,
                                                isTransp,
                                                faceType);

                        m_DataTable.Add(data.id, data);
                        count++;
                    }
                }

                if (ioDebug.VERBOSE_DEBUG_ACTIVE) DebugMaterialTableToConsole();

                ioDebug.Log("Loaded " + count + " materials from '" + _path + "'.");
            }
            catch (Exception e)
            {
                //TODO
                ioDebug.Log("IsThisNeeded-" + e.StackTrace + "\n" + e.Source + " : Exception-" + e.GetBaseException().Message);
            }
        }

        public static void SetData(ushort _id, Voxel _data)
        {
            if (m_DataTable.ContainsKey(_id))
            {
                m_DataTable[_id] = _data;
            }
            else
            {
                m_DataTable.Add(_id, _data);
            }
        }

        public static FaceType StringToFaceType(string _type)
        {
            string type = _type.ToUpper();

            if (Enum.IsDefined(typeof(FaceType), type))
            {
                return (FaceType)Enum.Parse(typeof(FaceType), type);
            }
            else
            {
                ioDebug.Log("Type invalid when trying to convert '" + type + "' to FaceType.  Using type [NONE=0].");
                //System.ArgumentException e = new System.ArgumentException("Invalid FaceType '" + type + "'");
                //throw e;
                return FaceType.NONE;
            }
        }
       
    }

    public enum FaceType : byte
    {
        NONE = 0,     //No face representation
        FLAT = 1,     //Flat 4 corners plane
        BEVJOIN = 2,  //Beveled edges with joined faces (NOT IMPLEMENTED)
        PILE = 3,     //(NOT IMPLEMENTED)
        CHAMF = 4,    //(NOT IMPLEMENTED)
        GLOB = 5      //(NOT IMPLEMENTED)
    }

    public enum NeighborJoinType : byte
    {
        NONE = 0,       
        FLAT = 1,
        LIFTED = 2  //(NOT IMPLEMENTED)
    }

    

}

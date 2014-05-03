using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioSS;
using System.Collections;
using CDataArray;
using CDataArray.ioVector;


namespace ioSS.ioVox
{
    class ioVoxChunk : IEnumerable<ioVoxChunk.VoxSurface>
    {
        
        #region Implementation of IEnumerable
        public IEnumerator<VoxSurface> GetEnumerator() //Surface Meshes
        {
            foreach (VoxSurface surf in m_Surfaces)
            {
                yield return surf;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public IVector3 RootCoord { get { return m_WorldRootCoord; } }

        private CDataArray3D m_Data;  //Reference Data

        private List<VoxSurface> m_Surfaces;
        private IVector3 m_WorldRootCoord;  //Xp, Yp, Zp
        private IVector3 m_Size;


        public ioVoxChunk(CDataArray3D _data, IVector3 _rootCoord, IVector3 _size)
        {
            m_Data = _data;
            m_WorldRootCoord = _rootCoord;
            m_Size = _size;

            m_Surfaces = new List<VoxSurface>();
        }

        /// <summary>
        /// Get Material ID of voxel at specified coord.
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns>Material Data (ushort)</returns>
        public ushort? this[int _x, int _y, int _z, bool _peek]
        {
            get
            {
                if (!ContainsCoord(new IVector3(_x, _y, _z), _peek))
                {
                    return null;
                }

                //IVector3 localCoord = ToLocal(new IVector3(_x, _y, _z));

                return m_Data[new IVector3(_x, _y, _z)];
            }

            //TODO
            //set { Update((ushort)value, new IVector3(_x, _y, _z)); }

        }
        public ushort? this[IVector3 _coord, bool _peek]
        {
            get
            {
                return this[_coord.x, _coord.y, _coord.z, _peek];
            }

            //TODO
            //set { Update((ushort)value, new IVector3(_x, _y, _z)); }

        }

        public bool ContainsCoord(IVector3 _coord, bool _peekAllowed = false)
        {
            IVector3 localCoord = ToLocal(_coord);

            if (!_peekAllowed)
            {
                if (localCoord.x >= m_Size.x || localCoord.y >= m_Size.y || localCoord.z >= m_Size.z ||
                    localCoord.x < 0 || localCoord.y < 0 || localCoord.z < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (localCoord.x >= m_Size.x + 1 || localCoord.y >= m_Size.y + 1 || localCoord.z >= m_Size.z + 1 ||
                    localCoord.x < -1 || localCoord.y < -1 || localCoord.z < -1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private IVector3 ToLocal(IVector3 _coord)
        {
            return _coord - m_WorldRootCoord;
        }

        /// <summary>
        /// Returns whether the source and target materials match and and if they both have faces in the specified direction.
        /// Source coordinate must be in bounds. Target can be a peek.
        /// </summary>
        /// <param name="_sourceCoord"></param>
        /// <param name="_sourceDir"></param>
        /// <param name="_targetCoord"></param>
        /// <param name="_targetDir"></param>
        /// <returns></returns>
        private bool FacesMatch(IVector3 _sourceCoord, IVector3 _sourceDir, IVector3 _targetCoord, IVector3 _targetDir)
        {
            if (IsFace(_sourceCoord, _sourceDir) && IsFace(_targetCoord, _targetDir))
            {
                return this[_sourceCoord, false] == this[_targetCoord, true] ? true : false;
            }
            else
            {
                return false;
            }
        }

        private bool IsFace(IVector3 _sourceCoord, IVector3 _direction)
        {
            IVector3 facingCoord = _sourceCoord + _direction;

            if(this[_sourceCoord, true] == null) return false;  //Source coord is invalid or not visible, no face

            ushort sourceMatID = (ushort)this[_sourceCoord, true];
            ushort? facingMatID = this[facingCoord,true];
            
			if(!VoxData.IsVisible(sourceMatID)) return false;
			
            if (facingMatID == null)  //Invalid material or out of bounds = no face
            {
                return true;
            }
            else
            {
                if (VoxData.IsTransparent(facingMatID.Value) ||
                    !VoxData.IsVisible(facingMatID.Value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
        }

        /// <summary>
        /// Returns true if the face is contained within m_Surfaces.
        /// </summary>
        /// <param name="_worldCoord"></param>
        /// <param name="_direction"></param>
        /// <returns></returns>
        private bool ContainsFace(IVector3 _worldCoord, IVector3 _direction)
        {
            foreach (VoxSurface surface in m_Surfaces)
            {
                if (surface.ContainsFace(_worldCoord, _direction)) return true;
            }
            return false;
        }

        /// <summary>
        /// Find and create all faces for this chunk.  This will delete any faces currently in the face list.
        /// </summary>
        public void BuildAllFaces()
        {
            m_Surfaces = new List<VoxSurface>();

            //Scan all coordinates for faces
            for (int x = m_WorldRootCoord.x; x < m_WorldRootCoord.x + m_Size.x; ++x)
                for (int y = m_WorldRootCoord.y; y < m_WorldRootCoord.y + m_Size.y; ++y)
                    for (int z = m_WorldRootCoord.z; z < m_WorldRootCoord.z + m_Size.z; ++z)
                    {
                        IVector3 worldCoord = new IVector3(x, y, z);
                        foreach (IVector3 dir in IVector3.Dirs3D)
                        {
                            if (IsFace(worldCoord, dir))
                                if(!ContainsFace(worldCoord, dir)) SurfaceBuilder(worldCoord, dir);
                        }
                    }

        }

        /// <summary>
        /// Compares the materials between a source and target.  If target matches material
        /// then the material at the next facing coordinate is checked for a lifted condition.
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="_target"></param>
        /// <param name="_facing"></param>
        /// <returns></returns>
        private NeighborJoinType GetJoinType(IVector3 _source, IVector3 _target, IVector3 _facing)
        {

            #region Sanity Check
            //Sanity Check
            bool sanityOK = true;
            if ((_facing == IVector3.Xp) || (_facing == IVector3.Xn))
            {
                if (Math.Abs(_source.x - _target.x) > 1) sanityOK = false;
            }
            else if ((_facing == IVector3.Yp) || (_facing == IVector3.Yn))
            {
                if (Math.Abs(_source.y - _target.y) > 1) sanityOK = false;
            }
            else if ((_facing == IVector3.Zp) || (_facing == IVector3.Zn))
            {
                if (Math.Abs(_source.z - _target.z) > 1) sanityOK = false;
            }
            else
            {
                ioDebug.Log("GetJoinType- Facing direction not valid!");
                throw new System.ArgumentOutOfRangeException("Facing", "Invalid direction " + _facing);
            }
            if (!sanityOK) ioDebug.Log("GetJoinType: Z values difference > 1.  Sanity Check fail!");
            #endregion

            ushort sourceMat = this[_source,false].Value;

            if (this[_target, true] == sourceMat) //Flat match?
            {
                if (this[_target + _facing, true] == sourceMat) //Lifted match?
                {
                    return NeighborJoinType.LIFTED;
                }
                else
                {
                    return NeighborJoinType.FLAT;
                }
            }
            else
            {
                return NeighborJoinType.NONE;
            }
        }

        /// <summary>
        /// Face building.  Assumes face is visible.
        /// </summary>
        /// <param name="_toWorldCoord"></param>
        /// <param name="_faceDir"></param>
        /// <param name="_fromWorldCoord"></param>
        private void SurfaceBuilder(IVector3 _worldCoord, IVector3 _faceDir)
        {
            ushort? faceMat = this[_worldCoord, false];
            
            //Get direction vectors for the plane this face lies on
            List<IVector3> planeDirs = IVector3.GetPlaneAxes(_faceDir);

            
            #region Find Width and Height
            //find largest rectangle height then width.
            int hp = 1, hn = 0, wp = 1, wn = 0;

            IVector3 scanCoord = new IVector3(_worldCoord);

            //TODO reduce all of this below to simpler less repeated code

            //Find rect width
            //Positive width direction
            while (true)
            {
                //if in bounds
                if (ContainsCoord(scanCoord + planeDirs[0]))
                {
					IVector3 nextCoord = scanCoord + planeDirs[0];
                    if ((this[nextCoord, false] == faceMat) && IsFace(nextCoord, _faceDir) && !ContainsFace(nextCoord, _faceDir))
                    {
                        wp++; scanCoord += planeDirs[0];
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            scanCoord = new IVector3(_worldCoord);

            //Width Negative Dir
            while (true)
            {
                //if in bounds
                if (ContainsCoord(scanCoord - planeDirs[0]))
                {
                    IVector3 nextCoord = scanCoord - planeDirs[0];
                    if ((this[nextCoord,false] == faceMat) && (IsFace(nextCoord, _faceDir)) && !ContainsFace(nextCoord, _faceDir))
                    {
                        wn++; scanCoord -= planeDirs[0];
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }


            bool scanDone = false;
            int peekH = 0;
            //Height Positive direction
            while(true)
            {
                peekH++;

                //if in bounds
                if (ContainsCoord(_worldCoord + (planeDirs[1] * peekH)))
                {
                    for (int widePos = -wn; widePos < wp; ++widePos)
                    {
                        scanCoord = _worldCoord + (planeDirs[0] * widePos) + (planeDirs[1] * peekH);
                        if ((faceMat != this[scanCoord,false]) || (!IsFace(scanCoord, _faceDir)) || ContainsFace(scanCoord, _faceDir))
                        {
                            scanDone = true;
                            break;
                        }
                    }
                    if (scanDone) break;
                    hp++;
                }
                else
                {
                    break;
                }
            }

            scanCoord = new IVector3(_worldCoord);
            peekH = 0;
            scanDone = false;
            //Height Negative direction
            while (true)
            {
                peekH++;

                //if in bounds
                if (ContainsCoord(_worldCoord - (planeDirs[1] * peekH)))
                {
                    for (int widePos = -wn; widePos < wp; ++widePos)
                    {
                        scanCoord = _worldCoord + (planeDirs[0] * widePos) - (planeDirs[1] * peekH);
                        if ((faceMat != this[scanCoord,false]) || !IsFace(scanCoord, _faceDir) || ContainsFace(scanCoord, _faceDir)) 
                        {
                            scanDone = true;
                            break;
                        }
                    }
                    if (scanDone) break;
                    hn++;
                }
                else
                {
                    break;
                }
            }
            #endregion

            IVector3 rootCoord = new IVector3(_worldCoord - (planeDirs[0] * wn) - (planeDirs[1] * hn));
            VoxSurface surf = new VoxSurface(VoxData.GetFaceType(faceMat.Value), rootCoord, wn + wp, hn + hp, _faceDir);

            //Build neighbor data if needed
            #region Add Neighbor Data
            if (VoxData.GetFaceType(faceMat.Value) == FaceType.BEVJOIN)
            {
                List<Dictionary<IVector3, NeighborJoinType>> edgeData = new List<Dictionary<IVector3, NeighborJoinType>>();
                Dictionary<IVector3, NeighborJoinType> diagonalData = new Dictionary<IVector3, NeighborJoinType>();
                Dictionary<IVector3, NeighborJoinType> upperEdgeData = new Dictionary<IVector3,NeighborJoinType>();
                Dictionary<IVector3, NeighborJoinType> rightEdgeData = new Dictionary<IVector3,NeighborJoinType>();
                Dictionary<IVector3, NeighborJoinType> lowerEdgeData = new Dictionary<IVector3,NeighborJoinType>();
                Dictionary<IVector3, NeighborJoinType> leftEdgeData = new Dictionary<IVector3,NeighborJoinType>();

                //Diagonals (UL, UR, LR, LL)
                IVector3 ulCorner = rootCoord + planeDirs[1] * (surf.Height - 1);
                IVector3 urCorner = rootCoord + (planeDirs[0] * (surf.Width - 1)) + (planeDirs[1] * (surf.Height - 1));
                IVector3 lrCorner = rootCoord + (planeDirs[0] * (surf.Width - 1));
                IVector3 llCorner = rootCoord;

                diagonalData.Add(ulCorner - planeDirs[0] + planeDirs[1], 
                    GetJoinType(ulCorner, ulCorner - planeDirs[0] + planeDirs[1], _faceDir));
                diagonalData.Add(urCorner + planeDirs[0] + planeDirs[1],
                    GetJoinType(urCorner, urCorner + planeDirs[0] + planeDirs[1], _faceDir));
                diagonalData.Add(lrCorner + planeDirs[0] - planeDirs[1],
                    GetJoinType(lrCorner, lrCorner + planeDirs[0] - planeDirs[1], _faceDir));
                diagonalData.Add(llCorner - planeDirs[0] - planeDirs[1],
                    GetJoinType(llCorner, llCorner - planeDirs[0] - planeDirs[1], _faceDir));

                NeighborJoinType? lastNbr = null;
                //Upper edge
                foreach (IVector3 edgeCoord in surf.GetEdgeCoordsCW(VoxSurface.Edge.UPPER))
                {
                    if (this[edgeCoord + planeDirs[1],true] == faceMat.Value) //Has flat neighbor?
                    {
                        if (this[edgeCoord + planeDirs[1] + _faceDir,true] == faceMat.Value) //Has lifted neighbor?
                        {
                            if((lastNbr == null) || (lastNbr != NeighborJoinType.LIFTED))
                                upperEdgeData.Add(edgeCoord, NeighborJoinType.LIFTED);
                            lastNbr = NeighborJoinType.LIFTED;
                        }
                        else
                        {
                            if ((lastNbr == null) || (lastNbr != NeighborJoinType.FLAT))
                                upperEdgeData.Add(edgeCoord, NeighborJoinType.FLAT);
                            lastNbr = NeighborJoinType.FLAT;
                        }
                    }
                    else
                    {
                        if ((lastNbr == null) || (lastNbr != NeighborJoinType.NONE))
                            upperEdgeData.Add(edgeCoord, NeighborJoinType.NONE);
                        lastNbr = NeighborJoinType.NONE;
                    }
                }

                //Right edge
                foreach (IVector3 edgeCoord in surf.GetEdgeCoordsCW(VoxSurface.Edge.RIGHT))
                {
                    if (this[edgeCoord + planeDirs[0],true] == faceMat.Value) //Has flat neighbor?
                    {
                        if (this[edgeCoord + planeDirs[0] + _faceDir,true] == faceMat.Value) //Has lifted neighbor?
                        {
                            rightEdgeData.Add(edgeCoord, NeighborJoinType.LIFTED);
                        }
                        else
                        {
                            rightEdgeData.Add(edgeCoord, NeighborJoinType.FLAT);
                        }
                    }
                    else
                    {
                        rightEdgeData.Add(edgeCoord, NeighborJoinType.NONE);
                    }
                }

                //Lower edge
                foreach (IVector3 edgeCoord in surf.GetEdgeCoordsCW(VoxSurface.Edge.LOWER))
                {
                    if (this[edgeCoord - planeDirs[1],true] == faceMat.Value) //Has flat neighbor?
                    {
                        if (this[edgeCoord - planeDirs[1] + _faceDir,true] == faceMat.Value) //Has lifted neighbor?
                        {
                            lowerEdgeData.Add(edgeCoord, NeighborJoinType.LIFTED);
                        }
                        else
                        {
                            lowerEdgeData.Add(edgeCoord, NeighborJoinType.FLAT);
                        }
                    }
                    else
                    {
                        lowerEdgeData.Add(edgeCoord, NeighborJoinType.NONE);
                    }
                }

                //left edge
                foreach (IVector3 edgeCoord in surf.GetEdgeCoordsCW(VoxSurface.Edge.LEFT))
                {
                    if (this[edgeCoord - planeDirs[0],true] == faceMat.Value) //Has flat neighbor?
                    {
                        if (this[edgeCoord - planeDirs[0] + _faceDir,true] == faceMat.Value) //Has lifted neighbor?
                        {
                            leftEdgeData.Add(edgeCoord, NeighborJoinType.LIFTED);
                        }
                        else
                        {
                            leftEdgeData.Add(edgeCoord, NeighborJoinType.FLAT);
                        }
                    }
                    else
                    {
                        leftEdgeData.Add(edgeCoord, NeighborJoinType.NONE);
                    }
                }

                edgeData.Add(upperEdgeData);
                edgeData.Add(rightEdgeData);
                edgeData.Add(lowerEdgeData);
                edgeData.Add(leftEdgeData);



                surf.SetNeighborData(edgeData, diagonalData);
            }

            #endregion


            surf.CreateMeshAtRoot(new IVector3(0,0,0));

            m_Surfaces.Add(surf);

        }


        public class VoxSurface
        {

            public FaceType FaceType { get { return m_FaceType; } }
            public List<FVector3> Verts { get { return m_Vertices; } }
            public List<int> Tris { get { return m_Tris; } }
            public List<float[]> UVs { get { return m_UVs; } }
            public List<FVector3> Normals { get { return m_Normals; } }
            public IVector3 Facing { get { return m_Facing; } }
            public IVector3 RootCoord { get { return m_RootWorldCoord; } }
            public int Width { get { return m_Width; } }
            public int Height { get { return m_Height; } }

            public ioMesh Mesh
            {
                get
                {
                    if (!m_IsMeshed)
                    {
                        ioDebug.Log("Surface mesh requested that has not been built.");
                        return new ioMesh(new List<FVector3>(), new List<float[]>(), new List<int>(), new List<FVector3>());
                    }
                    else
                    {
                        return new ioMesh(m_Vertices, m_UVs, m_Tris, m_Normals);
                    }
                }
            }

            private bool m_IsMeshed = false;

            private FaceType m_FaceType;

            private List<FVector3> m_Vertices;
            private List<int> m_Tris;
            private List<float[]> m_UVs;
            private List<FVector3> m_Normals;

            /// <summary>
            /// Lower left most data coordinate of surface
            /// </summary>
            private IVector3 m_RootWorldCoord;
            private int m_Height;
            private int m_Width;
            private IVector3 m_Facing;

            private List<Dictionary<IVector3, NeighborJoinType>> m_NeighborData = null;
            private Dictionary<IVector3, NeighborJoinType> m_NeighborDiagData = null;

            public enum Edge : byte
            {
                UPPER = 0,
                RIGHT = 1,
                LOWER = 2,
                LEFT = 3
            }

            public VoxSurface(FaceType _faceType, IVector3 _root, int _width, int _height, IVector3 _facing)
            {
                m_FaceType = _faceType;
                m_Vertices = new List<FVector3>();
                m_UVs = new List<float[]>();
                m_Tris = new List<int>();
                m_Normals = new List<FVector3>();
                m_Height = _height;
                m_Width = _width;
                m_RootWorldCoord = _root;
                m_Facing = _facing;
                m_IsMeshed = false;

            }

            public void ClearAll()
            {
                m_UVs.Clear();
                m_Tris.Clear();
                m_Vertices.Clear();
                m_Normals.Clear();
            }

            public int GetPlaneCoord()
            {
                int plane = m_RootWorldCoord.x * m_Facing.x;
                if (plane != 0) return plane;

                plane = m_RootWorldCoord.y * m_Facing.y;
                if (plane != 0) return plane;

                plane = m_RootWorldCoord.z * m_Facing.z;
                if (plane != 0) return plane;

                ioDebug.Log("ioVoxChunk:PlaneCoord- No plane coordinate found or coordinate is zero. returning 0");
                return 0;

            }
            
            public bool ContainsFace(IVector3 _worldCoord, IVector3 _dir)
            {
                //Check for direction match
                if (m_Facing != _dir) return false;


                //Localize to Surface
                IVector3 localCheckCoord = (IVector3)LocalizeWorldCoord(_worldCoord);

                if (localCheckCoord.z != 0) return false; //Shouldn't happen?


                if (localCheckCoord.x >= 0 &&
                    localCheckCoord.x < m_Width &&
                    localCheckCoord.y >= 0 &&
                    localCheckCoord.y < m_Height)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void CreateMeshAtRoot(IVector3 _root)
            {
                List<FVector3> planeDirs = IVector3.GetPlaneAxes((FVector3)m_Facing);

                FVector3 llCoord = (FVector3)_root;
                FVector3 ulCoord = ((FVector3)_root + (planeDirs[1] * (m_Height - 1)));
                FVector3 lrCoord = ((FVector3)_root + (planeDirs[0] * (m_Width - 1)));
                FVector3 urCoord = (ulCoord + (planeDirs[0] * (m_Width - 1)));

                

                #region FLAT Meshing
                if (m_FaceType == ioVox.FaceType.FLAT)
                {

                    //FLAT Meshing
                    FVector3 zChoord = (FVector3)m_Facing * 0.5f;
                    FVector3 ulLocal = (planeDirs[0] / -2f) + (planeDirs[1] / 2f) + zChoord;

                    FVector3 ul = ulCoord + ulLocal;
                    FVector3 ur = urCoord + MirrorFaceCW(ul);
                    FVector3 lr = lrCoord + MirrorFaceCW(ur);
                    FVector3 ll = llCoord + MirrorFaceCW(lr);


                    m_Vertices.Add(ul);
                    m_Vertices.Add(ur);
                    m_Vertices.Add(lr);
                    m_Vertices.Add(ll);

                    m_UVs = new List<float[]> { new float[] { 0, 0 }, 
                                                        new float[] { m_Width, 0 }, 
                                                        new float[] { m_Width, m_Height }, 
                                                        new float[] { 0, m_Height } 
                    };

                    m_Tris = new List<int>() { 0, 1, 2, 0, 2, 3 };

                    m_Normals = new List<FVector3>() { new FVector3((FVector3)m_Facing), 
                                                    new FVector3((FVector3)m_Facing), 
                                                    new FVector3((FVector3)m_Facing), 
                                                    new FVector3((FVector3)m_Facing) };

                    m_IsMeshed = true;
                }
                #endregion

                
                #region BEVELED JOINED meshing -- Not working
                /*

                if (m_FaceType == ioVox.FaceType.BEVJOIN)
                {
                    float edgeBnd = 0.45f;
                    float faceBnd = 0.4f;
                    float zBevDown = 0.45f;
                    float zFace = 0.5f;
                    float zBevUp = 0.55f;

                    //Create surface corner vertices
                    FVector3 ulDiagVert = CreateCornerVertHelper_BevJoin((IVector3)ulCoord,
                                                                         m_NeighborData[(int)Edge.UPPER][(IVector3)(ulCoord + planeDirs[1])],
                                                                         m_NeighborData[(int)Edge.LEFT][(IVector3)(ulCoord - planeDirs[0])],
                                                                         m_NeighborDiagData[(IVector3)(ulCoord - planeDirs[0] + planeDirs[1])],
                                                                         (IVector3)(-planeDirs[0]), (IVector3)planeDirs[1]);
                    FVector3 urDiagVert = CreateCornerVertHelper_BevJoin((IVector3)urCoord,
                                                                         m_NeighborData[(int)Edge.UPPER][(IVector3)(urCoord + planeDirs[1])],
                                                                         m_NeighborData[(int)Edge.RIGHT][(IVector3)(urCoord + planeDirs[0])],
                                                                         m_NeighborDiagData[(IVector3)(urCoord + planeDirs[0] + planeDirs[1])],
                                                                         (IVector3)planeDirs[0], (IVector3)planeDirs[1]);
                    FVector3 lrDiagVert = CreateCornerVertHelper_BevJoin((IVector3)lrCoord,
                                                                         m_NeighborData[(int)Edge.LOWER][(IVector3)(lrCoord - planeDirs[1])],
                                                                         m_NeighborData[(int)Edge.RIGHT][(IVector3)(lrCoord + planeDirs[0])],
                                                                         m_NeighborDiagData[(IVector3)(lrCoord + planeDirs[0] - planeDirs[1])],
                                                                         (IVector3)planeDirs[0], (IVector3)(-planeDirs[1]));
                    FVector3 llDiagVert = CreateCornerVertHelper_BevJoin((IVector3)ulCoord,
                                                                         m_NeighborData[(int)Edge.LOWER][(IVector3)(ulCoord - planeDirs[1])],
                                                                         m_NeighborData[(int)Edge.LEFT][(IVector3)(ulCoord - planeDirs[0])],
                                                                         m_NeighborDiagData[(IVector3)(ulCoord - planeDirs[0] - planeDirs[1])],
                                                                         (IVector3)(-planeDirs[0]), (IVector3)(-planeDirs[1]));


                    
                    List<FVector3> upperVerts = new List<FVector3>();

                    //TODO add face vertex

                    //NeighborJoinType lastNeighbor = m_NeighborData[(int)Edge.UPPER][(IVector3)ulCoord];
                    FVector3 lastEdge = new FVector3(ulCoord);
                    foreach (KeyValuePair<IVector3, NeighborJoinType> edge in m_NeighborData[(int)Edge.UPPER])
                    {
                        FVector3 faceVert, edgeVert;
                        IVector3 coord = new IVector3(edge.Key);

                        if (edge.Value == NeighborJoinType.NONE)
                        {
                            edgeVert = new FVector3(coord - (planeDirs[0] * edgeBnd) + (planeDirs[1] * edgeBnd) + ((FVector3)m_Facing * zBevDown));
                            faceVert = new FVector3(coord - (planeDirs[0] * faceBnd) + (planeDirs[1] * faceBnd) + ((FVector3)m_Facing * zFace));
                        }
                        else if ((edge.Value == NeighborJoinType.FLAT) || (edge.Value == NeighborJoinType.LIFTED))
                        {
                            edgeVert = new FVector3(coord - (planeDirs[0] * edgeBnd) + (planeDirs[1] * edgeBnd) + ((FVector3)m_Facing * zBevDown));
                            faceVert = new FVector3(coord - (planeDirs[0] * faceBnd) + (planeDirs[1] * faceBnd) + ((FVector3)m_Facing * zFace));
                        }
                    }

                    //FLAT
                    if ((m_NeighborData[(int)Edge.UPPER][(IVector3)ulCoord] == NeighborJoinType.FLAT) &&
                        (m_NeighborData[(int)Edge.LEFT][(IVector3)ulCoord] == NeighborJoinType.FLAT) &&
                        (m_NeighborDiagData[(IVector3)(ulCoord - planeDirs[0] + planeDirs[1])] == NeighborJoinType.FLAT))
                    {

                    }
                    

                    

                    //FVector3 ulEdge = new FVector3(ulCoord - (planeDirs[0] * 0.45f) + (planeDirs[1] * 0.45f) + (FVector3)m_Facing * 0.45f);
                    //FVector3 ulFace = new FVector3(ulCoord - (planeDirs[0] * 0.4f) + (planeDirs[1] * 0.4f) + (FVector3)m_Facing / 2);

                    FVector3 ulCorner = new FVector3(ulCoord - (planeDirs[0] * edgeBnd) + (planeDirs[1] * edgeBnd) + ((FVector3)m_Facing * zEdge));
                    FVector3 ulFace = new FVector3(ulCoord - (planeDirs[0] * faceBnd) + (planeDirs[1] * faceBnd) + (FVector3)m_Facing * zFace);

                    FVector3 urCorner = urCoord + MirrorFaceCW(ulCorner);
                    FVector3 urFace = urCoord + MirrorFaceCW(ulFace);

                    FVector3 lrCorner = lrCoord + MirrorFaceCW(urCorner);
                    FVector3 lrFace = lrCoord + MirrorFaceCW(urFace);

                    FVector3 llCorner = llCoord + MirrorFaceCW(lrCorner);
                    FVector3 llFace = llCoord + MirrorFaceCW(lrFace);

                    m_Vertices.AddRange(new List<FVector3>() { ulFace, ulCorner, urFace, urCorner, lrFace, lrCorner, llFace, llCorner });

                    //UVs
                    m_UVs = new List<float[]> { new float[] { 0.1f, 0.1f }, new float[] { 0f, 0f }, 
                                            new float[] { m_Width - 0.1f, 0.1f }, new float[] { m_Width, 0f }, 
                                            new float[] { m_Width - 0.1f, m_Height - 0.1f }, new float[] { m_Width, m_Height }, 
                                            new float[] { 0.1f, m_Height - 0.1f }, new float[] { 0f, m_Height } 
                    };

                    m_Tris = new List<int>() { 0, 1, 3, 0, 3, 2,
                                           2, 3, 5, 2, 5, 4,
                                           4, 5, 7, 4, 7, 6,
                                           6, 7, 1, 6, 1, 0,
                                           0, 2, 4, 0, 4, 6
                    };

                    m_Normals = new List<FVector3>() { new FVector3((planeDirs[0] * -0.301511f) + (planeDirs[1] * 0.301511f) + (FVector3)m_Facing * 0.904534f),
                                                   new FVector3((planeDirs[0] * -0.57735f) + (planeDirs[1] * 0.57735f) + (FVector3)m_Facing * 0.57735f),
                                                   
                                                   new FVector3((planeDirs[0] * 0.301511f) + (planeDirs[1] * 0.301511f) + (FVector3)m_Facing * 0.904534f),
                                                   new FVector3((planeDirs[0] * 0.57735f) + (planeDirs[1] * 0.57735f) + (FVector3)m_Facing * 0.57735f), 
                                                   
                                                   new FVector3((planeDirs[0] * 0.301511f) - (planeDirs[1] * 0.301511f) + (FVector3)m_Facing * 0.904534f),
                                                   new FVector3((planeDirs[0] * 0.57735f) - (planeDirs[1] * 0.57735f) + (FVector3)m_Facing * 0.57735f),  
                                                   
                                                   new FVector3((planeDirs[0] * -0.301511f) - (planeDirs[1] * 0.301511f) + (FVector3)m_Facing * 0.904534f),
                                                   new FVector3((planeDirs[0] * -0.57735f) - (planeDirs[1] * 0.57735f) + (FVector3)m_Facing * 0.57735f), 
                    };

                    m_IsMeshed = true;
                }
                 */
                #endregion
                
            }

            /// <summary>
            /// Returns local surface coordinate of provided world coordinate.  Local coordiante relative to surface origin (or provided one if specified).
            /// </summary>
            /// <param name="_toLocalize">World Coordinate to localize</param>
            /// <param name="_originW">(Optional) Local Origin point in world coordinates.  Current surface origin used otherwise. (Lower left corner)</param>
            /// <returns>Local coordinate of provided world coordinate</returns>
            public FVector3 LocalizeWorldCoord(FVector3 _toLocalize, IVector3 _originW = null)//, IVector3 _facing = m_Facing)
            {

                if (_originW == null) _originW = m_RootWorldCoord;

                IVector3 facing = m_Facing;
                FVector3 localVec = _originW - _toLocalize;

                if (!IVector3.IsValidDirection(facing))
                {
                    throw new ArgumentException("Localize Vector- Direction " + facing + " not a cardinal direction.");
                }

                if (facing == IVector3.Xp)
                {
                    return new FVector3(-localVec.z, -localVec.y, -localVec.x);
                }
                else if (facing == IVector3.Xn)
                {
                    return new FVector3(localVec.z, -localVec.y, localVec.x);
                }
                else if (facing == IVector3.Yp)
                {
                    return new FVector3(-localVec.x, -localVec.z, -localVec.y);
                }
                else if (facing == IVector3.Yn)
                {
                    return new FVector3(localVec.x, -localVec.z, localVec.y);
                }
                else if (facing == IVector3.Zn)
                {
                    return new FVector3(-localVec.x, -localVec.y, localVec.z);
                }
                else if (facing == IVector3.Zp)
                {
                    return new FVector3(localVec.x, -localVec.y, -localVec.z);
                }
                else
                {
                    ioDebug.Log("LocalizeTo - Invalid direction: " + facing);
                    return null;
                }
            }

            /// <summary>
            /// Returns a copy of vectors at world coord Mirrored to the next CW quadrant of the face.
            /// </summary>
            /// <param name="_targetFace"></param>
            /// <param name="_mirrorPoint"></param>
            /// <param name="_count"></param>
            /// <returns></returns>
            public FVector3 MirrorFaceCW(FVector3 _targetFace, int _count = 1)
            {

                IVector3 facing = m_Facing;

                /*if (!IVector3.IsValidDirection(facing))
                {
                    throw new ArgumentException("Rotate90 FVector- Direction " + facing + " not a cardinal direction.");
                }*/

                FVector3 localVec = LocalizeWorldCoord(_targetFace, (IVector3)_targetFace);

                for (int cnt = 0; cnt < _count; ++cnt)
                {
                    if (localVec.x < 0) //Left half
                    {
                        if (localVec.y > 0) //Upper half
                        {
                            localVec.x = -localVec.x;
                        }
                        else //Lower half
                        {
                            localVec.y = -localVec.y;
                        }
                    }
                    else  //Right half
                    {
                        if (localVec.y > 0) //Upper half
                        {
                            localVec.y = -localVec.y;
                        }
                        else //Lower half
                        {
                            localVec.x = -localVec.x;
                        }
                    }
                }

                return new FVector3(LocalToWorldAxes(localVec));
            }


            /// <summary>
            /// Converts local coordinate and axes to world coordinates and axes.  Transforms to world origin if provided.
            /// </summary>
            /// <param name="_localC"></param>
            /// <param name="_originW"></param>
            /// <returns></returns>
            public FVector3 LocalToWorldAxes(FVector3 _localC, FVector3 _originW = null)//, IVector3 _facingW)
            {
                if (_originW == null) _originW = IVector3.Zero;

                IVector3 facing = m_Facing;

                if (!IVector3.IsValidDirection(facing))
                {
                    throw new ArgumentException("Localize Vector- Direction " + facing + " not a cardinal direction.");
                }

                if (facing == IVector3.Xp)
                {
                    return _originW + new FVector3(_localC.z, _localC.y, _localC.x);
                }
                else if (facing == IVector3.Xn)
                {
                    return _originW + new FVector3(-_localC.z, _localC.y, -_localC.x);
                }
                else if (facing == IVector3.Yp)
                {
                    return _originW + new FVector3(_localC.x, _localC.z, _localC.y);
                }
                else if (facing == IVector3.Yn)
                {
                    return _originW + new FVector3(-_localC.x, -_localC.z, _localC.y);
                }
                else if (facing == IVector3.Zn)
                {
                    return _originW + new FVector3(_localC.x, _localC.y, -_localC.z);
                }
                else if (facing == IVector3.Zp)
                {
                    return _originW + new FVector3(-_localC.x, _localC.y, _localC.z);
                }
                else
                {
                    ioDebug.Log("LocalToWorldAxes - Invalid facing direction: " + facing);
                    return null;
                }
            }

            public void SetNeighborData(List<Dictionary<IVector3, NeighborJoinType> > _edgeData, Dictionary<IVector3, NeighborJoinType> _diagData)
            {
                m_NeighborData = _edgeData;
                m_NeighborDiagData = _diagData;
            }

            public List<IVector3> GetEdgeCoordsCW(Edge _edge)
            {
                List<IVector3> coords = new List<IVector3>();
                List<IVector3> planeDirs = IVector3.GetPlaneAxes(m_Facing);

                if (_edge == Edge.UPPER)
                {
                    for (int count = 0; count < m_Width; ++count)
                    {
                        coords.Add(new IVector3(RootCoord + (planeDirs[0] * count) + (planeDirs[1] * (m_Height - 1))));
                    }
                }
                else if (_edge == Edge.RIGHT)
                {
                    for (int count = m_Height - 1; count >= 0; --count)
                    {
                        coords.Add(new IVector3(RootCoord + (planeDirs[0] * (m_Width - 1)) + (planeDirs[1] * count)));
                    }
                }
                else if (_edge == Edge.LOWER)
                {
                    for (int count = m_Width - 1; count >= 0; --count)
                    {
                        coords.Add(new IVector3(RootCoord + (planeDirs[0] * count)));
                    }
                }
                else if (_edge == Edge.LEFT)
                {
                    for (int count = 0; count < m_Height; ++count)
                    {
                        coords.Add(new IVector3(RootCoord + (planeDirs[1] * count)));
                    }
                }

                return coords;
            }

            private FVector3 CreateCornerVertHelper_BevJoin(IVector3 _coord, NeighborJoinType _join1, NeighborJoinType _join2, NeighborJoinType _diagJoin, IVector3 _xDir, IVector3 _yDir)
            {
                //float xyFacePos = 0.4f;
                float xyEdgePos = 0.45f;
                float xyLimitPos = 0.5f;

                float zFace = 0.5f;
                float zLow = 0.45f;
                float zLowLim = 0.4f;

                if (_join1 == NeighborJoinType.NONE && _join2 == NeighborJoinType.NONE)
                    return new FVector3(_coord + ((FVector3)_xDir * xyEdgePos) + ((FVector3)_yDir * xyEdgePos) + ((FVector3)m_Facing * zLow));

                if ((_join1 == NeighborJoinType.NONE && _join2 == NeighborJoinType.FLAT) ||
                    (_join1 == NeighborJoinType.FLAT && _join2 == NeighborJoinType.NONE) ||
                    (_join1 == NeighborJoinType.FLAT && _join2 == NeighborJoinType.FLAT))
                {
                    if (_diagJoin == NeighborJoinType.NONE)
                        return new FVector3(_coord + ((FVector3)_xDir * xyLimitPos) + ((FVector3)_yDir * xyLimitPos) + ((FVector3)m_Facing * zLowLim));
                    if (_diagJoin == NeighborJoinType.FLAT || _diagJoin == NeighborJoinType.LIFTED)
                        return new FVector3(_coord + ((FVector3)_xDir * xyLimitPos) + ((FVector3)_yDir * xyLimitPos) + ((FVector3)m_Facing * zFace));
                }
                //Catch everything else as FLAT
                //TODO check all cases
                return new FVector3(_coord + ((FVector3)_xDir * xyLimitPos) + ((FVector3)_yDir * xyLimitPos) + ((FVector3)m_Facing * zFace));

            }
        }
    }


    

    
}

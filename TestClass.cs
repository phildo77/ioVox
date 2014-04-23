using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioSS.ioVox;
using ioSS;

    class TestClass
    {
        static void Main(string[] Args)
        {
            //Test xml read
            CDataArray3D data = new CDataArray3D(new IVector3(50,50,50));
            ioVoxChunk testChunk = new ioVoxChunk(data, new IVector3(0, 0, 0), new IVector3(10, 10, 10));

            VoxData.LoadDataFromFile("C:\\Users\\plorenz\\workspace\\ioSS\\ioVOX\\ioSS\\ioVox\\DefaultType.xml");

        }
    }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Collections;

namespace MyFirstGame
{
    class ObjModel
    {
        Vector3[] verts;
        int[] inds;

        List<Vector3> verts_temp = new List<Vector3>();
        List<int> inds_temp = new List<int>();

        public Vector3[] Vertices
        {
            get { return verts; }
        }

        public int[] Indices
        {
            get { return inds; }
        }

        public ObjModel(string filename)
        {
            Load(filename);
        }

        public void Load(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                    ParseString(str);
            }

            verts = verts_temp.ToArray();
            inds = inds_temp.ToArray();
            verts_temp.Clear();
            inds_temp.Clear();
        }

        private void ParseString(string str)
        {
            string[] blocks = str.Split(new char[]{' '});
            if (blocks[0] == "v")
                ParseVertex(blocks);
            else if (blocks[0] == "f")
                ParseFace(blocks);
        }

        private void ParseVertex(string[] blocks)
        {
            Vector3 v = new Vector3(float.Parse(blocks[1]), float.Parse(blocks[2]), float.Parse(blocks[3]));
            verts_temp.Add(v);
        }

        private void ParseFace(string[] blocks)
        {
            for (int i = blocks.Length - 1; i > 0; --i)
                ParseIndex(blocks[i]);
        }

        private void ParseIndex(string str)
        {
            string[] blocks = str.Split(new char[]{'/'});
            inds_temp.Add(int.Parse(blocks[0]) - 1);
        }
    }
}

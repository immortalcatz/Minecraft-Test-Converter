using MineServer.Map.Format;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftWorldConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: MinecraftWorldConverter.exe [path] [optional(default:256):sectorSize]");
                return;
            }

            short sectorSize = 256;
            if (args.Length > 2)
            {
                sectorSize = short.Parse(args[1]);
            }

            LevelLoader loader = new LevelLoader(args[0]);

            var str = new MemoryStream();

            NewLevelWriter writer = new NewLevelWriter(str, sectorSize);
            for (int x = 0; x < 32; x++)
            {
                for (int z = 0; z < 32; z++)
                {
                    var tag = loader.Read(x, z);

                    if (tag != null)
                    {
                        var compound = (tag as NbtCompound).GetValue<NbtCompound>("Level");

                        for (int y = 0; y < 16; y++)
                        {
                            NbtCompound root = new NbtCompound { Name = "Level" };
                            root.Childs = new List<NbtNode>();
                            //entities
                            NbtList entities = compound.GetValue<NbtList>("Entities");
                            NbtList newEntities = new NbtList { Name = "Entities", TagId = entities.TagId};
                            root.Childs.Add(newEntities);
                            newEntities.Childs = new List<object>();
                            foreach (var item in entities.Childs)
                            {
                               var list = item as List<NbtNode>;
                               var position = list.Single(a => a.Name == "Pos") as NbtList;
                               var locationY = (double)position.Childs[1];

                               if (locationY > y * 16 && locationY < (y + 1) * 16)
                               {
                                   newEntities.Childs.Add(item);
                               }
                            }
                            root.Childs.Add(compound.Childs.Single(a => a.Name == "Biomes"));
                            root.Childs.Add(compound.Childs.Single(a => a.Name == "LastUpdate"));
                            root.Childs.Add(compound.Childs.Single(a => a.Name == "xPos"));
                            root.Childs.Add(new NbtInt { Name = "yPos", Value = y });
                            root.Childs.Add(compound.Childs.Single(a => a.Name == "zPos"));

                            //tile entities
                            NbtList tileEntities = compound.GetValue<NbtList>("TileEntities");
                            NbtList newTileEntities = new NbtList { Name = "TileEntities", TagId = tileEntities.TagId };
                            root.Childs.Add(newTileEntities);
                            newTileEntities.Childs = new List<object>();
                            foreach (var item in tileEntities.Childs)
                            {
                                var list = item as List<NbtNode>;
                                var position = list.Single(a => a.Name == "y") as NbtInt;
                                var locationY = position.Value;

                                if (locationY > y * 16 && locationY < (y + 1) * 16)
                                {
                                    newTileEntities.Childs.Add(item);
                                }
                            }
                            root.Childs.Add(compound.Childs.Single(a => a.Name == "HeightMap"));


                            //tile entities
                            NbtList data = compound.GetValue<NbtList>("Sections");
                            bool b = false;
                            foreach (var item in data.Childs)
                            {
                                var list = item as List<NbtNode>;
                                var position = list.Single(a => a.Name == "Y") as NbtByte;
                                var locationY = position.Value;

                                if (locationY == y)
                                {
                                    b = true;
                                    NbtCompound newData = new NbtCompound { Name = "Section" };
                                    newData.Childs = new List<NbtNode>();
                                    foreach (var it in list)
                                    {
                                        newData.Childs.Add(it);
                                    }
                                    root.Childs.Add(newData);
                                }
                            }
                            if (!b)
                            {

                            }

                            writer.Write(x, (short)y, z, new NbtCompound { Childs = new List<NbtNode> { root } });
                        }
                    }
                }
                
            }
            File.WriteAllBytes(args[0] +"." + sectorSize +  ".new", str.ToArray());

            Console.WriteLine("Maximum: " + writer._sizes.Max());
            Console.WriteLine("Minimum: " + writer._sizes.Min());
            Console.WriteLine("Average: " + writer._sizes.Average());
        }
        
    }
}

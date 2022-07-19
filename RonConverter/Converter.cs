using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassEffectModManagerCore.modmanager.save.game3;
using static MassEffectModManagerCore.modmanager.save.game3.MorphHead;

namespace RonConverter
{
    public static class Converter
    {
        public const string DefaultFemaleRon = "female default.ron";
        public const string DefaultMaleRon = "male default.ron";

        public static async Task FullCustomHead(string inputFile, string outputFile, Gender gender, string customMorphName)
        {
            var morphHead = RONConverter.ConvertRON(inputFile);
            var baseDefaultMorphHead = RONConverter.ConvertRON(gender == Gender.Female ? DefaultFemaleRon : DefaultMaleRon);

            if (string.IsNullOrWhiteSpace(customMorphName))
            {
                customMorphName = Path.GetFileNameWithoutExtension(inputFile);
            }

            var morphTargetOutput = ConvertMorphToCoalesced(morphHead, baseDefaultMorphHead, baseDefaultMorphHead, customMorphName);

            var templateOutput = ConvertToCoalescedFormat(morphHead, gender);

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                outputFile = Path.ChangeExtension(inputFile, "txt");
            }

            await File.WriteAllTextAsync(outputFile, morphTargetOutput + "\r\n\r\n\r\n" + templateOutput);
        }

        public static async Task GenerateMorph(string inputFile, string outputFile, Gender gender, string alternateBase = null)
        {
            var morphHead = RONConverter.ConvertRON(inputFile);
            var baseMorphHead = RONConverter.ConvertRON(alternateBase ?? (gender == Gender.Female ? DefaultFemaleRon : DefaultMaleRon));
            var baseDefaultMorphHead = RONConverter.ConvertRON(gender == Gender.Female ? DefaultFemaleRon : DefaultMaleRon);

            var outputString = ConvertMorphToCoalesced(morphHead, baseMorphHead, baseDefaultMorphHead, "test");

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                outputFile = Path.ChangeExtension(inputFile, "txt");
            }

            await File.WriteAllTextAsync(outputFile, outputString);
        }

        public static async Task Convert(string inputFile, string outputFile, Gender gender)
        {
            var morphHead = RONConverter.ConvertRON(inputFile);

            var outputString = ConvertToCoalescedFormat(morphHead, gender);

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                outputFile = Path.ChangeExtension(inputFile, "txt");
            }

            await File.WriteAllTextAsync(outputFile, outputString);
        }

        private static string ConvertMorphToCoalesced(MorphHead target, MorphHead baseMorphHead, MorphHead defaultBaseMorphHead, string morphTargetName)
        {
            List<OffsetBone> boneDiffs = new List<OffsetBone>();
            List<(int sourceIndex, Vector offset)> vertexDiffs = new List<(int, Vector)>();

            foreach(var targetBone in target.OffsetBones)
            {
                var baseBone = baseMorphHead.OffsetBones.FirstOrDefault(bone => bone.Name == targetBone.Name);
                if (baseBone == null)
                {
                    baseBone = defaultBaseMorphHead.OffsetBones.FirstOrDefault(bone => bone.Name == targetBone.Name);
                }

                if (baseBone != null && !VectorEquals(targetBone.Offset, baseBone.Offset))
                {
                    boneDiffs.Add(new OffsetBone()
                    {
                        Name = targetBone.Name,
                        Offset = VectorSubtract(targetBone.Offset, baseBone.Offset)
                    });
                }
            }

            if (target.Lod0Vertices.Count != baseMorphHead.Lod0Vertices.Count)
            {
                throw new System.Exception("cannot compare vertices when vertex count differs");
            }
            for (int i = 0; i < target.Lod0Vertices.Count; i++)
            {
                var vertex = target.Lod0Vertices[i];
                var baseVert = baseMorphHead.Lod0Vertices[i];

                if (!VectorEquals(vertex, baseVert))
                {
                    vertexDiffs.Add((i, VectorSubtract(vertex, baseVert)));
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine(@$"<Value type=""3"">(TargetName=""{morphTargetName}"",");

            for (int i = 0; i < boneDiffs.Count; i++)
            {
                var boneDiff = boneDiffs[i];
                sb.AppendLine($@"BoneOffsets[{i}]=(Bone=""{boneDiff.Name}"",Offset=(x={boneDiff.Offset.X:f8},y={boneDiff.Offset.Y:f8},z={boneDiff.Offset.Z:f8})),");
            }

            sb.AppendLine($"LodModels[0]=(NumBaseMeshVertices={baseMorphHead.Lod0Vertices.Count},");
            for (int i = 0; i < vertexDiffs.Count; i++)
            {
                var (sourceIndex, offset) = vertexDiffs[i];
                sb.Append($@"vertices[{i}]=(sourceIndex={sourceIndex}, PositionDelta=(x={offset.X:f8},y={offset.Y:f8},z={offset.Z:f8})),");
            }
            sb.AppendLine(")");


            sb.AppendLine(")</Value>");
            return sb.ToString();
        }

        private static bool VectorEquals(Vector V1, Vector V2)
        {
            return V1.X == V2.X && V1.Y == V2.Y && V1.Z == V2.Z;
        }

        private static Vector VectorSubtract(Vector V1, Vector V2)
        {
            return new Vector()
            {
                X = V1.X - V2.X,
                Y = V1.Y - V2.Y,
                Z = V1.Z - V2.Z,
            };
        }

        private static string ConvertToCoalescedFormat(MorphHead morphHead, Gender gender)
        {
            var sb = new StringBuilder();

            sb.AppendLine(@$"<Value type=""3"">(Gender=Gender_{gender},ChoiceEntry=(srChoiceName=614208),Delta=(");

            // Hair Mesh
            sb.AppendLine(@$"HairMesh=""{(string.IsNullOrWhiteSpace(morphHead.HairMesh) ? "None" : morphHead.HairMesh)}"",");

            // accessory meshes
            sb.AppendLine(@"AccessoryMeshDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.AccessoryMeshes.Count; i++)
            {
                var accessory = morphHead.AccessoryMeshes[i];
                sb.AppendLine(@$"AccessoryMeshDeltas[{i+1}]=(Name=""{accessory}""),");
            }

            // Morph Features
            sb.AppendLine(@"MorphFeatureDeltas[0]=(Feature=""*"",Remove=true),");
            for (int i = 0; i < morphHead.MorphFeatures.Count; i++)
            {
                var morphFeature = morphHead.MorphFeatures[i];
                sb.AppendLine(@$"MorphFeatureDeltas[{i+1}]=(Feature=""{morphFeature.Feature}"",Offset=""{morphFeature.Offset}""),");
            }

            // Offset Bones
            //sb.AppendLine(@"OffsetBoneDeltas[0]=(Name=""*"",Remove=true),");
            //for (int i = 0; i < morphHead.OffsetBones.Count; i++)
            //{
            //    var bone = morphHead.OffsetBones[i];
            //    sb.AppendLine(@$"OffsetBoneDeltas[{i+1}]=(Name=""{bone.Name}"",Offset=(X=""{bone.Offset.X:F15}"",Y=""{bone.Offset.Y:F15}"",Z=""{bone.Offset.Z:F15}"")),");
            //}

            // vertices
            //for (int i = 0; i < morphHead.Lod0Vertices.Count; i++)
            //{
            //    var vertex = morphHead.Lod0Vertices[i];
            //    sb.Append(@$"LOD0Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            //}
            //sb.AppendLine();

            //for (int i = 0; i < morphHead.Lod1Vertices.Count; i++)
            //{
            //    var vertex = morphHead.Lod1Vertices[i];
            //    sb.Append(@$"LOD1Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            //}
            //sb.AppendLine();

            //for (int i = 0; i < morphHead.Lod2Vertices.Count; i++)
            //{
            //    var vertex = morphHead.Lod2Vertices[i];
            //    sb.Append(@$"LOD2Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            //}
            //sb.AppendLine();

            //for (int i = 0; i < morphHead.Lod3Vertices.Count; i++)
            //{
            //    var vertex = morphHead.Lod3Vertices[i];
            //    sb.Append(@$"LOD3Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            //}
            //sb.AppendLine();

            // scalar params
            sb.AppendLine(@"ScalarParameterDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.ScalarParameters.Count; i++)
            {
                var scalar = morphHead.ScalarParameters[i];
                sb.AppendLine(@$"ScalarParameterDeltas[{i+1}]=(Name=""{scalar.Name}"",Value=""{scalar.Value:F8}""),");
            }

            // Vector Params
            sb.AppendLine(@"VectorParameterDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.VectorParameters.Count; i++)
            {
                var vector = morphHead.VectorParameters[i];
                sb.AppendLine(@$"VectorParameterDeltas[{i+1}]=(Name=""{vector.Name}"",Value=(R=""{vector.Value.R:F8}"",G=""{vector.Value.G:F8}"",B=""{vector.Value.B:F8}"",A=""{vector.Value.A:F8}"")),");
            }

            // Texture params
            sb.AppendLine(@"TextureParameterDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.TextureParameters.Count; i++)
            {
                var tex = morphHead.TextureParameters[i];
                sb.AppendLine(@$"TextureParameterDeltas[{i+1}]=(Name=""{tex.Name}"",Texture={tex.Value}),");
            }

            sb.Append("))\r\n</Value>");

            return sb.ToString();
        }
    }
}

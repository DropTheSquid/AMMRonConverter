using System.IO;
using System.Text;
using System.Threading.Tasks;
using MassEffectModManagerCore.modmanager.save.game3;

namespace RonConverter
{
    public static class Converter
    {
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
            sb.AppendLine(@"OffsetBoneDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.OffsetBones.Count; i++)
            {
                var bone = morphHead.OffsetBones[i];
                sb.AppendLine(@$"OffsetBoneDeltas[{i+1}]=(Name=""{bone.Name}"",Offset=(X=""{bone.Offset.X:F15}"",Y=""{bone.Offset.Y:F15}"",Z=""{bone.Offset.Z:F15}"")),");
            }

            // vertices
            for (int i = 0; i < morphHead.Lod0Vertices.Count; i++)
            {
                var vertex = morphHead.Lod0Vertices[i];
                sb.Append(@$"LOD0Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            }
            sb.AppendLine();

            for (int i = 0; i < morphHead.Lod1Vertices.Count; i++)
            {
                var vertex = morphHead.Lod1Vertices[i];
                sb.Append(@$"LOD1Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            }
            sb.AppendLine();

            for (int i = 0; i < morphHead.Lod2Vertices.Count; i++)
            {
                var vertex = morphHead.Lod2Vertices[i];
                sb.Append(@$"LOD2Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            }
            sb.AppendLine();

            for (int i = 0; i < morphHead.Lod3Vertices.Count; i++)
            {
                var vertex = morphHead.Lod3Vertices[i];
                sb.Append(@$"LOD3Vertices[{i}]=(X={vertex.X:F15},Y={vertex.Y:F15},Z={vertex.Z:F15}),");
            }
            sb.AppendLine();

            // scalar params
            sb.AppendLine(@"ScalarParameterDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.ScalarParameters.Count; i++)
            {
                var scalar = morphHead.ScalarParameters[i];
                sb.AppendLine(@$"ScalarParameterDeltas[{i+1}]=(Name=""{scalar.Name}"",Value=""{scalar.Value}""),");
            }

            // Vector Params
            sb.AppendLine(@"VectorParameterDeltas[0]=(Name=""*"",Remove=true),");
            for (int i = 0; i < morphHead.VectorParameters.Count; i++)
            {
                var vector = morphHead.VectorParameters[i];
                sb.AppendLine(@$"VectorParameterDeltas[{i+1}]=(Name=""{vector.Name}"",Value=(R=""{vector.Value.R * 255}"",G=""{vector.Value.G * 255}"",B=""{vector.Value.B * 255}"",A=""{vector.Value.A * 255}"")),");
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

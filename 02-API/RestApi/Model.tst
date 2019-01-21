${
    using Typewriter.Extensions.Types;
    using System.Text.RegularExpressions;
    using System.Diagnostics;

    string ToKebabCase(string typeName){
        return  Regex.Replace(typeName, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])","-$1", RegexOptions.Compiled)
                     .Trim().ToLower();
    }

    string CleanupName(string propertyName, bool? removeArray = true){
        if (removeArray.HasValue && removeArray.Value) {
            propertyName = propertyName.Replace("[]","");
        }
        return propertyName;
    }

    Template(Settings settings)
    {
        settings.OutputFilenameFactory = (file) => {
            if (file.Classes.Any()){
                var className = file.Classes.First().Name;
                className = CleanupName(className);
                className = ToKebabCase(className);
                return $"../../01-Presentation/Mobile/src/Model/{className}.ts";
            }
            if (file.Enums.Any()){
                var className = file.Enums.First().Name;
                className = ToKebabCase(className);
                return $"../../01-Presentation/Mobile/src/Model/{className}.ts";
            }
            return file.Name;
        };
        settings.IncludeCurrentProject();
    }

    string Imports(Class c) => c.Properties
                                .Where(p=>!p.Type.IsPrimitive || p.Type.IsEnum)
                                .Select(p=> $"import {{ {CleanupName(p.Type.Name)} }} from './{ToKebabCase(CleanupName(p.Type.Name))}';")
                                .Aggregate("", (all,import) => $"{all}{import}\r\n")
                                .TrimStart();

    string CustomProperties(Class c) => c.Properties
                                        .Select(p=> $"\tpublic {p.name}: {CleanupName(p.Type.Name, false)};")
                                        .Aggregate("", (all,prop) => $"{all}{prop}\r\n")
                                        .TrimEnd();

    string ClassName(Class c) => c.Name.Replace("Model","");

}$Classes(*Model)[$Imports
export class $ClassName   {
$CustomProperties
}]$Enums(*)[export enum $Name { $Values[
    $name = $Value][,]
}]
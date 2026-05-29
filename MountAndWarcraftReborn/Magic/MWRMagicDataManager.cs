using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic
{
    public static class MWRMagicDataManager
    {
        private const string ModuleId = "MountAndWarcraftReborn";
        private const string ModuleDataFolder = "ModuleData";
        private const string SpellTemplateFileName = "mwr_magic_abilitytemplates.xml";
        private const string ClassDefinitionFileName = "mwr_magic_classes.xml";
        private const string AssignmentFileName = "mwr_magic_assignments.xml";

        private static readonly object SyncRoot = new object();

        private static bool _isLoaded;
        private static Dictionary<string, MWRMagicSpellTemplate> _spellTemplates = new(StringComparer.Ordinal);
        private static Dictionary<MWRMagicClassId, MWRMagicClassDefinition> _classDefinitions = new();
        private static Dictionary<string, MWRMagicAssignment> _assignments = new(StringComparer.Ordinal);

        public static IReadOnlyDictionary<string, MWRMagicSpellTemplate> SpellTemplates
        {
            get
            {
                EnsureLoaded();
                return _spellTemplates;
            }
        }

        public static IReadOnlyDictionary<MWRMagicClassId, MWRMagicClassDefinition> ClassDefinitions
        {
            get
            {
                EnsureLoaded();
                return _classDefinitions;
            }
        }

        public static void EnsureLoaded()
        {
            if (_isLoaded)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_isLoaded)
                {
                    return;
                }

                _spellTemplates = LoadSpellTemplates();
                _classDefinitions = LoadClassDefinitions();
                _assignments = LoadAssignments();
                _isLoaded = true;
            }
        }

        public static MWRMagicSpellTemplate? GetSpellTemplate(string spellId)
        {
            EnsureLoaded();
            return spellId != null && _spellTemplates.TryGetValue(spellId, out MWRMagicSpellTemplate template)
                ? template
                : null;
        }

        public static MWRMagicClassDefinition? GetClassDefinition(MWRMagicClassId classId)
        {
            EnsureLoaded();
            return _classDefinitions.TryGetValue(classId, out MWRMagicClassDefinition definition)
                ? definition
                : null;
        }

        public static MWRMagicAssignment? GetAssignment(string characterId)
        {
            EnsureLoaded();
            return !string.IsNullOrWhiteSpace(characterId) && _assignments.TryGetValue(characterId, out MWRMagicAssignment assignment)
                ? assignment
                : null;
        }

        private static Dictionary<string, MWRMagicSpellTemplate> LoadSpellTemplates()
        {
            SpellTemplateCollection collection = Deserialize<SpellTemplateCollection>(GetModuleDataPath(SpellTemplateFileName));
            return collection.Spells
                .Where(spell => !string.IsNullOrWhiteSpace(spell.StringId))
                .GroupBy(spell => spell.StringId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        private static Dictionary<MWRMagicClassId, MWRMagicClassDefinition> LoadClassDefinitions()
        {
            ClassDefinitionCollection collection = Deserialize<ClassDefinitionCollection>(GetModuleDataPath(ClassDefinitionFileName));
            return collection.Classes
                .Where(definition => definition.ClassId != MWRMagicClassId.None)
                .GroupBy(definition => definition.ClassId)
                .ToDictionary(group => group.Key, group => group.First());
        }

        private static Dictionary<string, MWRMagicAssignment> LoadAssignments()
        {
            AssignmentCollection collection = Deserialize<AssignmentCollection>(GetModuleDataPath(AssignmentFileName));
            return collection.Assignments
                .Where(assignment => !string.IsNullOrWhiteSpace(assignment.CharacterId))
                .GroupBy(assignment => assignment.CharacterId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        private static T Deserialize<T>(string filePath)
            where T : class, new()
        {
            if (!File.Exists(filePath))
            {
                return new T();
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using FileStream stream = File.OpenRead(filePath);
                return serializer.Deserialize(stream) as T ?? new T();
            }
            catch (Exception exception)
            {
                TaleWorlds.Library.Debug.Print($"MWR magic data load failed for {filePath}: {exception}");
                return new T();
            }
        }

        private static string GetModuleDataPath(string fileName)
        {
            return Path.Combine(BasePath.Name, "Modules", ModuleId, ModuleDataFolder, fileName);
        }

        [XmlRoot("MagicAbilityTemplates")]
        public class SpellTemplateCollection
        {
            [XmlElement("Spell")]
            public List<MWRMagicSpellTemplate> Spells { get; set; } = new();
        }

        [XmlRoot("MagicClasses")]
        public class ClassDefinitionCollection
        {
            [XmlElement("Class")]
            public List<MWRMagicClassDefinition> Classes { get; set; } = new();
        }

        [XmlRoot("MagicAssignments")]
        public class AssignmentCollection
        {
            [XmlElement("Assignment")]
            public List<MWRMagicAssignment> Assignments { get; set; } = new();
        }
    }
}

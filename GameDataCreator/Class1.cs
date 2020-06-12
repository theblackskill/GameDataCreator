﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameDataCreator
{
    // Unity calls this function when the user accepts an edited name
    internal class EndNameEdit : EndNameEditAction
    {
        #region implemented abstract members of EndNameEditAction
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
        }

        #endregion
    }
    public class Class1 : EditorWindow
    {
        string SchemaName = "";

        string[] rows;
        string nameOfFile;
        public string[,] arrayTypeName;
        string content;
        string path;

        // Scriptable "Factory"
        private int selectedIndex;
        private static string[] names;

        private static Type[] types;

        private static Type[] Types
        {
            get { return types; }
            set
            {
                types = value;
                names = types.Select(t => t.FullName).ToArray();
            }
        }
        private int toolbarInt = 0;
        private string[] toolbarNames = { "Create", "Populate" };
        public List<data1> objects = new List<data1>();


        
        // Returns the assembly that contains the script code for this project (currently hard coded)
        private static Assembly GetAssembly()
        {
            return Assembly.Load(new AssemblyName("Assembly-CSharp"));
        }
        void GetAllScriptableObjects()
        {
            var assembly = GetAssembly();

            // Get all classes derived from ScriptableObject
            var allScriptableObjects = (from t in assembly.GetTypes()
                                        where t.IsSubclassOf(typeof(ScriptableObject))
                                        select t).ToArray();

            Types = allScriptableObjects;
        }

        [MenuItem("Window/GameDataCreator_ToCompile")]
        public static void ShowWindow()
        {
            var assembly = GetAssembly();

            // Get all classes derived from ScriptableObject
            var allScriptableObjects = (from t in assembly.GetTypes()
                                        where t.IsSubclassOf(typeof(ScriptableObject))
                                        select t).ToArray();

            Types = allScriptableObjects;

            GetWindow<Class1>(true, "GameDataCreator_ToCompile", true);
        }

        void OnGUI()
        {
            toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarNames, new GUILayoutOption[0]);
            switch (toolbarInt)
            {
                case 0:
                    Create();
                    break;
                case 1:
                    Populate();
                    break;
            }
        }

        private void Create()
        {
            SchemaName = EditorGUILayout.TextField("Schema Name: ", SchemaName);
            EditorGUILayout.Space(); // Divisor

            if (GUILayout.Button("Load CSV file"))
            {
                nameOfFile = "data1";
                TextAsset data = Resources.Load<TextAsset>(nameOfFile);
                rows = data.text.Split(new char[] { '\n' });
                //Debug.Log(rows.Length);
                //Debug.Log(rows[0]);

                string[] dataType = rows[0].Split(new char[] { ';' });
                string[] dataName = rows[1].Split(new char[] { ';' });
                string[] dataTest = rows[2].Split(new char[] { ';' });
                //Debug.Log("dataTest[0]: " + dataTest[2]);
                arrayTypeName = new string[dataType.Length, 2];
                for (int i = 0; i < dataType.Length; i++)
                {
                    arrayTypeName[i, 0] = dataType[i];
                    Debug.Log("dataType[" + i + "]: " + dataType[i]);
                    //Debug.Log("arrayTypeName[" + i + ", " + 0 + "]: " + arrayTypeName[i, 0]);
                    for (int j = 1; j < 2; j++)
                    {
                        //Debug.Log("dataName[" + i + "]: " + dataName[i]);
                        arrayTypeName[i, j] = dataName[i];
                        //Debug.Log("arrayTypeName[" + i + ", " + j + "]: " + arrayTypeName[i, j]);
                    }
                }
                for (int i = 0; i < dataType.Length; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Debug.Log("arrayTypeName[" + i + ", " + j + "]: " + arrayTypeName[i, j]);
                    }
                }

                path = Application.dataPath + "/" + nameOfFile + ".cs";
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "using UnityEngine;\n\n" +
                       "[CreateAssetMenu(fileName = \"New " + SchemaName + "\", menuName = \"" + SchemaName + "\")]\n" + // fileName = name variable
                                                                                                                         // menuName = ask user input
                        "public class " + nameOfFile + " : ScriptableObject\n" +
                        "{\n");
                    Debug.Log("File created!");
                }

                for (int i = 0; i < dataType.Length; i++)
                {
                    string temp;
                    for (int j = 0; j < 1; j++)
                    {
                        temp = "\tpublic " + arrayTypeName[i, j] + " " + arrayTypeName[i, j + 1] + ";\n";
                        Debug.Log(temp);
                        content = temp;
                    }
                    File.AppendAllText(path, content);
                }
                File.AppendAllText(path, "\n}");

                AssetDatabase.Refresh();
            }

            EditorGUILayout.Space(); // Divisor

            // Refreshes all the ScriptableObjects search to find the new one just created
            GetAllScriptableObjects();

            Debug.Log("names.Length: " + names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == nameOfFile)
                {
                    selectedIndex = i;
                }
            }
            if (GUILayout.Button("Create"))
            {
                for (int i = 2; i < rows.Length - 1; i++)
                {
                    var asset = ScriptableObject.CreateInstance(types[selectedIndex]);
                    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                        asset.GetInstanceID(),
                        ScriptableObject.CreateInstance<EndNameEdit>(),
                        string.Format("{0}.asset", SchemaName), // SchemaName name
                        AssetPreview.GetMiniThumbnail(asset),
                        null);
                    objects.Add((data1)asset);
                }
                //Debug.Log(objects[1]);
            }
        }

        private void Populate()
        {
            string[] data;
            int id;
            float hp;
            float damage;
            bool god;
            if (GUILayout.Button("Populate"))
            {
                Debug.Log(objects.Count);
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] != null)
                    {
                        for (int j = 2; j < rows.Length - 1; j++)
                        {
                            data = rows[j].Split(new char[] { ';' });
                            //for (int k = 0; k < 1; k++)
                            //{
                            int.TryParse(data[0], out id);
                            float.TryParse(data[2], out hp);
                            float.TryParse(data[3], out damage);
                            bool.TryParse(data[4], out god);
                            objects[i].id = id;
                            objects[i].name = data[1];
                            objects[i].hp = hp;
                            objects[i].damage = damage;
                            objects[i].god = god;
                            //}
                            //int.TryParse(data[0], out id);
                            //objects[i].id = id;
                            //objects[i].id = 1;
                            //objects[i].hp = 2;
                            //objects[i].damage = 3;

                            BinaryFormatter binaryFormatter = new BinaryFormatter();
                            FileStream file = File.Create(Application.persistentDataPath + string.Format("/{0}.pso", i));
                            var json = JsonUtility.ToJson(objects[i]);
                            binaryFormatter.Serialize(file, json);
                            file.Close();

                            if (File.Exists(Application.persistentDataPath + string.Format("/{0}.pso", i)))
                            {
                                file = File.Open(Application.persistentDataPath + string.Format("/{0}.pso", i), FileMode.Open);
                                JsonUtility.FromJsonOverwrite((string)binaryFormatter.Deserialize(file), objects[i]);
                                file.Close();
                                // writing changes of the testScriptable into Undo
                                Undo.RecordObject(objects[i], "Changed Data");
                                // mark the ScriptableObject object as "dirty" and save it
                                EditorUtility.SetDirty(objects[i]);
                            }
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}

using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

/// <summary>
/// Класс для создания скриншотов в редакторе юнити
/// </summary>
namespace ScreenShotMaker
{
    public class EditorScreenShotMaker : EditorWindow
    {
        private string version = "V 1.0.0";

        private Vector2 scrollPosGlobal = Vector2.zero;
        private Vector2 scrollPosEditor = Vector2.zero;

        private List<ClassScenes> classScenes = new List<ClassScenes>();
        private ClassScreenShot classScreenShot = null;

        private float minTimeScale = Math.Abs(float.MinValue);
        private float maxTimeScale = 2f;

        private bool isActiveEditor = false;
        private bool isEditorSave = false;
        private bool isActiveTimeScale = true;
        private bool isActiveScenes = true;
        private bool isActiveScreenShot = true;

        private bool isViewTimeScale = false;
        private bool isViewTimeScaleEdit = false;
        private bool isViewScenes = false;
        private bool isViewScenesChange = false;
        private bool isViewScreenShot = false;
        private bool isViewScreenShotParams = false;
        private bool isFixTimeScale = false;
        private bool isScreenShotDisableInterface = false;

        #region StringsTutors
        private string tutorGlobal = "\tEditorHelper - предназначен для упрощения разработки и тестирования приложений. " +
                                     "Внизу есть вкладка Editor где можно включать, отключать и настраивать имеющиеся функции.\n" +
                                     "\tБольшинство параметров окна сохраняются в EditorPrefs и привзяваются к productName, " +
                                     "поэтому при смене названия имейте ввиду, что почти все настройки редактора собьются.";
        private string tutorTimeScale = "\tУменьшение TimeScale позволяет (в большинстве случаев) замедлять игровой " +
                                        "процесс. В настройках можно выставить минимальное и максимальное значение. " +
                                        "По умолчанию минимальное значение стоит 0.00001f, тк при 0 аппа может ставиться на пазу.";
        private string tutorScenes = "\tВкладка Scenes позволяет быстро переходить между сценами. По умолчанию редактор " +
                                     "подтягивает сцены забитые в BuildSettings. В настройках можно добавлять, удалять и " +
                                     "переименовывать сцены.";
        private string tutorAutoSave = "\tВкладка AutoSaveScene позволяет в процессе разработки автоматических сохранять " +
                                       "сцену. Выставите интервал сохранения. Галочка Use Notification AutoSave отвечает " +
                                       "за окно подтверждения сохранения, Use AutoSave за включение функции автосохранения.";
        private string tutorClearPrefs = "\tВкладка с кнопкой очистки префсов. Содержит скрытую кнопки очистки EditorPrefs " +
                                         "(Использовать в случае крайней необходимости!)";
        private string tutorScreenShots = "\tВо вкладке ScreenShot есть кнопка для создания скриншотов разных разрешений " +
                                          "одним нажатием.\n" +
                                          "\t1) Выберите папку куда будут сохраняться скрины. Если её не задавать они будут по " +
                                          "умолчанию сохраняться в папку Screenshots в папке с проектом. Адреса папок сохранятся " +
                                          "идивидуально для каждого проекта.\n" +
                                          "\t2) Добавьте разрешение для которых надо сделать скрины. Разрешения добавляются " +
                                          "на все платформы и хранятся в EditorPrefs, поэтому будут кочевать в другие проекты, на " +
                                          "другие платформы и на другие версии юнити. Они добавляются в редактор при запуске " +
                                          "EditorHelper и проверяются/добавляются при сохранении скриншота.\n" +
                                          "\t3) Если надо сделать скрины для конкретного разрешения или группы, уберите галочки " +
                                          "у неактуальных разрешений.\n" +
                                          "\t4) Галочка DisableInterface отвечает за отключение интерфейса при создании скрина.\n" +
                                          "\tPS: Скрины делаются с задержкой 0,5 секунд, тк они не сразу сохраняются.";
        private string tutorCheats = "\tВкладка Cheats отвечает за быстрое изменение префсов. \n" +
                                     "\t1) В настройках можно добавлять префсы руками поштучно. (Add cheat)\n" +
                                     "\t2) Найти автоматически в проекте. (Find all prefs). " +
                                     "Автоматически префсы находятся только с указанием простых ключей (PlayerPrefs.SetInt(\"Gold\",gold)).\n" +
                                     "\t Скрипт умеет работать как с PlayerPrefs так и с PlayerPrefsHelper (если в нем есть SetBool!). Для использования " +
                                     "второго надо добавить директиву PLAYER_PREFS_HELPER, она добавляется автоматически при открытие " +
                                     "окна. Но если вдруг надо удалить или добавить есть кнопка FIX PLAYER_PREFS_HELPER!";
        #endregion StringsTutors

        #region StartMethods
        [MenuItem("Tools/ScreenShotMaker")]
        /// <summary>
        /// Инициализация
        /// Обязательно должна быть статичной!!!!
        /// </summary>
        private static void Init()
        {
            EditorScreenShotMaker window = (EditorScreenShotMaker)EditorWindow.GetWindow(typeof(EditorScreenShotMaker));
            window.title = "ScreenShotMaker";
            window.Show();
        }
        /*
        /// <summary>
        /// Показываем окно
        /// </summary>
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(EditorScreenShotMaker));
        }*/

        private void Awake()
        {
            classScreenShot = new ClassScreenShot();
            CheckClassScene();
            CheckScreenShots();
            LoadEditorParams();
        }

        /// <summary>
        /// Чистим информацию о кнопках в EditorPrefs, 
        /// скорей всего будет использоваться только для тестов
        /// </summary>
        private void ClearPrefsButtonScene()
        {
            Debug.Log("<color=red>ClearPrefsButtonScene</color>");
            for (int i = 0; i < 100; i++)
            {
                if (EditorPrefs.HasKey(Application.productName + "PathScene" + i))
                {
                    EditorPrefs.DeleteKey(Application.productName + "NameScene" + i);
                    EditorPrefs.DeleteKey(Application.productName + "PathScene" + i);
                }
                else
                {
                    i = 100;
                }
            }
        }

        /// <summary>
        /// Проверяем есть ли у нас параметры кнопок, для смены сцен
        /// </summary>
        private void CheckClassScene()
        {
            //Для тестов, пусть лежит тут
            //ClearPrefsButtonScene();
            if (!EditorPrefs.HasKey(Application.productName + "PathScene0"))
            {
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (EditorBuildSettings.scenes.Length == 0)
                    {
                        i = 100;
                    }

                    ClassScenes tempClassScenes = new ClassScenes();
                    string tempPath = EditorBuildSettings.scenes[i].path;
                    string[] tempArrayPath = tempPath.Split('/');
                    string tempName = tempArrayPath[tempArrayPath.Length - 1].Replace(".unity", "");

                    tempClassScenes.PathScene = tempPath.Replace(".unity", "");
                    tempClassScenes.NameScene = tempName;
                    tempClassScenes.SceneObject = AssetDatabase.LoadAssetAtPath(tempPath, typeof(object));

                    classScenes.Add(tempClassScenes);
                }
            }
            else
            {
                for (int i = 0; i < 100; i++)
                {
                    if (EditorPrefs.HasKey(Application.productName + "PathScene" + i))
                    {
                        ClassScenes tempClassScenes = new ClassScenes();
                        tempClassScenes.NameScene = EditorPrefs.GetString(Application.productName + "NameScene" + i);
                        tempClassScenes.PathScene = EditorPrefs.GetString(Application.productName + "PathScene" + i);
                        tempClassScenes.SceneObject = AssetDatabase.LoadAssetAtPath(tempClassScenes.PathScene + ".unity", typeof(object));
                        classScenes.Add(tempClassScenes);
                    }
                    else
                    {
                        i = 100;
                    }
                }
            }
        }

        /// <summary>
        /// Загружаем параметры скриншотов
        /// </summary>
        private void CheckScreenShots()
        {
            classScreenShot.PathFolderForScreenShot = EditorPrefs.GetString(Application.productName + "PathForScreenShots");
            for (int i = 0; i < 100; i++)
            {
                if (EditorPrefs.HasKey("Resolution" + i + "NameResolution"))
                {
                    ClassResolution tempClassResolution = new ClassResolution();
                    classScreenShot.M_ClassResolutionScreenShots.Add(tempClassResolution);

                    classScreenShot.M_ClassResolutionScreenShots[i].NameResolution =
                    EditorPrefs.GetString("Resolution" + i + "NameResolution");
                    classScreenShot.M_ClassResolutionScreenShots[i].Width =
                    EditorPrefs.GetInt("Resolution" + i + "Width");
                    classScreenShot.M_ClassResolutionScreenShots[i].Height =
                    EditorPrefs.GetInt("Resolution" + i + "Height");

                    AddResolution(classScreenShot.M_ClassResolutionScreenShots[i].NameResolution,
                    classScreenShot.M_ClassResolutionScreenShots[i].Width,
                    classScreenShot.M_ClassResolutionScreenShots[i].Height);
                }
                else
                {
                    i = 100;
                }
            }
        }
        #endregion StartMethods

        #region Updates
        /// <summary>
        /// Отрисовка гуя
        /// </summary>
        private void OnGUI()
        {
            scrollPosGlobal = GUILayout.BeginScrollView(scrollPosGlobal);

            ViewGuiScenesButtons();
            ViewGuiScreenShot();
            ViewGuiTimeScale();

            GUILayout.EndScrollView();

            ViewEditor();
        }

        private void OnInspectorUpdate()
        {
            TryMakeScreenShot();
        }
        #endregion Updates

        #region Editor
        /// <summary>
        /// Метод редактирования окна
        /// </summary>
        private void ViewEditor()
        {
            GUILayout.FlexibleSpace();
            scrollPosEditor = GUILayout.BeginScrollView(scrollPosEditor);
            GUILayout.Label("------------------------", EditorStyles.boldLabel);
            isActiveEditor = GUILayout.Toggle(isActiveEditor,
                                                (isActiveEditor == true ? "↑  " : "↓  ") + "Editor",
                                                EditorStyles.boldLabel);
            if (isActiveEditor)
            {
                GUILayout.Label(version, EditorStyles.boldLabel);
                ViewEditorTimeScale();
                ViewEditorScenes();
                ViewEditorScreenShot();

                if (isEditorSave)
                {
                    isEditorSave = false;
                }
            }
            else
            {
                if (!isEditorSave)
                {
                    SaveEditorParams();
                }
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Отрисовываем настройки тайм скейла
        /// </summary>
        private void ViewEditorTimeScale()
        {
            EditorGUILayout.BeginHorizontal();
            isActiveTimeScale = GUILayout.Toggle(isActiveTimeScale, "isActiveTimeScale");

            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                            tutorTimeScale,
                                            "Ok");
            }
            EditorGUILayout.EndHorizontal();
            if (isActiveTimeScale)
            {
                TimeScaleEditor();
            }
        }

        /// <summary>
        /// Отрисовываем настройки сцен
        /// </summary>
        private void ViewEditorScenes()
        {
            EditorGUILayout.BeginHorizontal();
            isActiveScenes = GUILayout.Toggle(isActiveScenes, "isActiveScenes");
            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                            tutorScenes,
                                            "Ok");
            }
            EditorGUILayout.EndHorizontal();
            if (isActiveScenes)
            {
                ViewScenesChange();
            }
        }

        /// <summary>
        /// Отрисовываем настройки скринов
        /// </summary>
        private void ViewEditorScreenShot()
        {
            EditorGUILayout.BeginHorizontal();
            isActiveScreenShot = GUILayout.Toggle(isActiveScreenShot, "isActiveScreenShot");
            if (GUILayout.Button("?", GUILayout.MaxWidth(30.0f)))
            {
                EditorUtility.DisplayDialog("",
                                            tutorScreenShots,
                                            "Ok");
            }
            EditorGUILayout.EndHorizontal();
            if (isActiveScreenShot)
            {
                ViewScreenShotParams();
            }
        }

        /// <summary>
        /// Сохраняем какие функции мы будем использовать
        /// </summary>
        private void SaveEditorParams()
        {
            isEditorSave = true;
            EditorPrefs.SetBool(Application.productName + "isActiveTimeScale", isActiveTimeScale);
            EditorPrefs.SetBool(Application.productName + "isActiveScenes", isActiveScenes);
            EditorPrefs.SetBool(Application.productName + "isActiveScreenShot", isActiveScreenShot);
        }

        /// <summary>
        /// Загружаем какие функции мы будем использовать
        /// </summary>
        private void LoadEditorParams()
        {
            isEditorSave = false;
            isActiveTimeScale = EditorPrefs.GetBool(Application.productName + "isActiveTimeScale", true);
            isActiveScenes = EditorPrefs.GetBool(Application.productName + "isActiveScenes", true);
            isActiveScreenShot = EditorPrefs.GetBool(Application.productName + "isActiveScreenShot", true);
        }
        #endregion Editor

        #region TimeScaleMethods
        /// <summary>
        /// Отрисовка слайдера тайм скейла
        /// </summary>
        private void ViewGuiTimeScale()
        {
            if (isActiveTimeScale)
            {
                isViewTimeScale = GUILayout.Toggle(isViewTimeScale,
                (isViewTimeScale == true ? "↑  " : "↓  ") + "Change TimeScale",
                EditorStyles.boldLabel);
                if (isViewTimeScale)
                {
                    Time.timeScale = EditorGUILayout.Slider(Time.timeScale, minTimeScale, maxTimeScale);
                    Time.fixedDeltaTime = 0.02F * Time.timeScale;
                }
            }
        }

        /// <summary>
        /// Редактор тайм скейла
        /// </summary>
        private void TimeScaleEditor()
        {
            isViewTimeScaleEdit = GUILayout.Toggle(isViewTimeScaleEdit,
                (isViewTimeScaleEdit == true ? "↑↑  " : "↓↓  ") + "TimeScaleEditor",
                EditorStyles.boldLabel);

            if (isViewTimeScaleEdit)
            {
                minTimeScale = EditorGUILayout.FloatField("MinTimeScale = ", minTimeScale);
                maxTimeScale = EditorGUILayout.FloatField("MaxTimeScale = ", maxTimeScale);
            }
        }
        #endregion TimeScaleMethods

        #region ScenesMethods
        /// <summary>
        /// Отрисовка кнопок переключения сцен
        /// </summary>
        private void ViewGuiScenesButtons()
        {
            if (isActiveScenes)
            {
                isViewScenes = GUILayout.Toggle(isViewScenes,
                                                (isViewScenes == true ? "↑  " : "↓  ") + "Scenes",
                                                EditorStyles.boldLabel);
                if (isViewScenes)
                {
                    for (int i = 0; i < classScenes.Count; i++)
                    {
                        if (GUILayout.Button(classScenes[i].NameScene))
                        {
                            LoadScene(classScenes[i].PathScene, classScenes[i].NameScene);
                        }
                        GUILayout.Space(10f);
                    }
                }
            }
        }

        /// <summary>
        /// Изменяем сцены
        /// </summary>
        private void ViewScenesChange()
        {
            isViewScenesChange = GUILayout.Toggle(isViewScenesChange,
                                                          (isViewScenesChange == true ? "↑↑  " : "↓↓  ") + "ScenesChange",
                                                          EditorStyles.boldLabel);
            if (isViewScenesChange)
            {
                for (int i = 0; i < classScenes.Count; i++)
                {
                    classScenes[i].NameScene = EditorGUILayout.TextField("NameScene" + i + ": ",
                                                                         classScenes[i].NameScene);

                    classScenes[i].SceneObject = EditorGUILayout.ObjectField(classScenes[i].SceneObject,
                                                                             typeof(UnityEngine.Object),
                                                                             true);

                    GUILayout.Space(10f);
                }

                if (GUILayout.Button("Add new scene"))
                {
                    Debug.Log("<color=red>Назначь сцену для новой кнопки</color>");
                    ClassScenes temp = new ClassScenes();

                    temp.NameScene = "New scene " + classScenes.Count;
                    temp.PathScene = "";
                    temp.SceneObject = null;

                    classScenes.Add(temp);
                }

                if (GUILayout.Button("Save all scene"))
                {
                    for (int i = 0; i < classScenes.Count; i++)
                    {
                        if (classScenes[i].SceneObject)
                        {
                            string tempPath = AssetDatabase.GetAssetPath(classScenes[i].SceneObject).Replace(".unity", "");
                            classScenes[i].PathScene = tempPath;
                            EditorPrefs.SetString(Application.productName + "PathScene" + i, tempPath);
                            EditorPrefs.SetString(Application.productName + "NameScene" + i, classScenes[i].NameScene);
                        }
                        else
                        {
                            Debug.Log("<color=red>Назначь сцену для кнопки </color>" + i);
                        }
                    }
                }

                if (GUILayout.Button("Delete last scene"))
                {
                    if (EditorUtility.DisplayDialog("Удаление последней сцены из списка",
                                                    "",
                                                    "ДА", "НЕТ"))
                    {
                        if (classScenes.Count > 0)
                        {
                            EditorPrefs.DeleteKey(Application.productName + "PathScene" + (classScenes.Count - 1));
                            EditorPrefs.DeleteKey(Application.productName + "NameScene" + (classScenes.Count - 1));
                            classScenes.RemoveAt(classScenes.Count - 1);
                        }
                    }
                }
                GUILayout.Label("------------------------", EditorStyles.boldLabel);
            }
        }

        /// <summary>
        /// Загрузка сцены
        /// </summary>
        private void LoadScene(string path, string name)
        {
            if (path != "")
            {
                if (!EditorApplication.isPlaying)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path + ".unity");
                    }
                }
                else
                {
                    LoadSceneIsPlayingMode(name);
                }
            }
            else
            {
                Debug.Log("<color=red>Не назначена сцена</color>");
            }
        }

        /// <summary>
        /// Загрузка сцены в плей моде
        /// </summary>
        private void LoadSceneIsPlayingMode(string sceneName)
        {
            Application.LoadLevel(sceneName);
        }
        #endregion ScenesMethods

        #region ScreenShotMethods
        /// <summary>
        /// Отрисовываем окно для скринов
        /// </summary>
        private void ViewGuiScreenShot()
        {
            if (isActiveScreenShot)
            {
                isViewScreenShot = GUILayout.Toggle(isViewScreenShot,
                                                    (isViewScreenShot == true ? "↑  " : "↓  ") + "ScreenShot",
                                                    EditorStyles.boldLabel);
                if (isViewScreenShot)
                {
                    isScreenShotDisableInterface = GUILayout.Toggle(isScreenShotDisableInterface,
                                                    "DisableInterface");

                    if (classScreenShot.PathFolderForScreenShot != "")
                    {
                        classScreenShot.PathFolderForScreenShot =
                        EditorGUILayout.TextField("PathFolder: ",
                            classScreenShot.PathFolderForScreenShot);
                    }
                    else
                    {
                        GUILayout.Label("Если ты не укажешь папку\n" +
                                        "куда сохранять скрины\n" +
                                        "они по умолчанию сохраняться\n" +
                                        "в Screenshots в папке с проектом", EditorStyles.boldLabel);
                    }

                    if (classScreenShot.M_ClassResolutionScreenShots.Count > 0)
                    {
                        GUILayout.Label("Resolution for ScreenShots:", EditorStyles.boldLabel);
                        for (int i = 0; i < classScreenShot.M_ClassResolutionScreenShots.Count; i++)
                        {
                            //GUILayout.Label(classScreenShot.M_ClassResolutionScreenShots[i].NameResolution +
                            //"  (" + classScreenShot.M_ClassResolutionScreenShots[i].Width + "/" +
                            //classScreenShot.M_ClassResolutionScreenShots[i].Height + ")");
                            classScreenShot.M_ClassResolutionScreenShots[i].isActive =
                            GUILayout.Toggle(classScreenShot.M_ClassResolutionScreenShots[i].isActive,
                                                "  " + classScreenShot.M_ClassResolutionScreenShots[i].NameResolution +
                                                "  (" + classScreenShot.M_ClassResolutionScreenShots[i].Width + "/" +
                                                classScreenShot.M_ClassResolutionScreenShots[i].Height + ")");
                        }
                    }
                    else
                    {
                        GUILayout.Label("Not resolutions for ScreenShots", EditorStyles.boldLabel);
                    }

                    if (GUILayout.Button("Make Screenshot"))
                    {
                        ActiveScreenShot(true);
                    }
                }
            }
        }

        /// <summary>
        /// Запускаем процесс делания скринов
        /// </summary>
        private void ActiveScreenShot(bool isActive)
        {
            if (isActive)
            {
                classScreenShot.LastTime = EditorApplication.timeSinceStartup - 0.6f;
                classScreenShot.CurrentTimeScale = Time.timeScale;
                Time.timeScale = 0.000001f;
                classScreenShot.CurrentScreenNumber = -1;
                if (isViewTimeScale)
                {
                    isFixTimeScale = true;
                    isViewTimeScale = false;
                }
            }
            else
            {
                Time.timeScale = classScreenShot.CurrentTimeScale;
                if (isFixTimeScale)
                {
                    isFixTimeScale = false;
                    isViewTimeScale = true;
                }
            }
            CheckDisableInterface(isActive);
            classScreenShot.IsActiveScreen = isActive;
        }

        /// <summary>
        /// Отрисовываем настройки скринов
        /// </summary>
        private void ViewScreenShotParams()
        {
            isViewScreenShotParams = GUILayout.Toggle(isViewScreenShotParams,
                                                       (isViewScreenShotParams == true ? "↑↑  " : "↓↓  ") + "ScreenShotParams",
                                                       EditorStyles.boldLabel);
            if (isViewScreenShotParams)
            {


                if (GUILayout.Button("Add Folder For ScreenShots"))
                {
                    classScreenShot.PathFolderForScreenShot = EditorUtility.OpenFolderPanel("Folder For ScreenShots", "", "");
                    EditorPrefs.SetString(Application.productName + "PathForScreenShots", classScreenShot.PathFolderForScreenShot);
                }

                classScreenShot.NameResolution =
                EditorGUILayout.TextField("NameResolution = ", classScreenShot.NameResolution);
                classScreenShot.Width =
                EditorGUILayout.IntField("Width = ", classScreenShot.Width);
                classScreenShot.Height =
                EditorGUILayout.IntField("Height = ", classScreenShot.Height);

                GUILayout.Space(10f);

                if (GUILayout.Button("Add Resolution"))
                {
                    if (classScreenShot.Width != 0 &&
                        classScreenShot.Height != 0 &&
                        classScreenShot.NameResolution != "")
                    {
                        AddResolution();
                    }
                    else
                    {
                        Debug.Log("<color=red>Заполни все поля для нового разрешения</color>");
                    }
                }

                if (GUILayout.Button("Delete last Resolution"))
                {
                    if (EditorUtility.DisplayDialog("",
                         "Удаляем последнее разрешение?",
                         "Удалить",
                         "Отмена"))
                    {
                        if (classScreenShot.M_ClassResolutionScreenShots.Count > 0)
                        {
                            EditorPrefs.DeleteKey("Resolution" +
                            (classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "NameResolution");
                            EditorPrefs.DeleteKey("Resolution" +
                            (classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Width");
                            EditorPrefs.DeleteKey("Resolution" +
                            (classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Height");

                            classScreenShot.M_ClassResolutionScreenShots.RemoveAt(classScreenShot.M_ClassResolutionScreenShots.Count - 1);
                        }
                    }
                }
                GUILayout.Label("------------------------", EditorStyles.boldLabel);
            }
        }

        /// <summary>
        /// Добавляем новое разрешение
        /// </summary>
        private void AddResolution()
        {
            if (ClassScreenShot.FindSize(ClassScreenShot.GetCurrentGroupType(), classScreenShot.NameResolution) == -1)
            {
                ClassScreenShot.AddCustomSize(ClassScreenShot.GetCurrentGroupType(),
                    classScreenShot.Width,
                    classScreenShot.Height,
                    classScreenShot.NameResolution);

                ClassResolution tempClassResolution = new ClassResolution();
                classScreenShot.M_ClassResolutionScreenShots.Add(tempClassResolution);

                classScreenShot.M_ClassResolutionScreenShots
                [classScreenShot.M_ClassResolutionScreenShots.Count - 1].Width = classScreenShot.Width;

                classScreenShot.M_ClassResolutionScreenShots
                [classScreenShot.M_ClassResolutionScreenShots.Count - 1].Height = classScreenShot.Height;

                classScreenShot.M_ClassResolutionScreenShots
                [classScreenShot.M_ClassResolutionScreenShots.Count - 1].NameResolution = classScreenShot.NameResolution;

                EditorPrefs.SetString("Resolution" + (classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "NameResolution",
                classScreenShot.NameResolution);
                EditorPrefs.SetInt("Resolution" + (classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Width",
                classScreenShot.Width);
                EditorPrefs.SetInt("Resolution" + (classScreenShot.M_ClassResolutionScreenShots.Count - 1) + "Height",
                classScreenShot.Height);

                classScreenShot.Width = 0;
                classScreenShot.Height = 0;
                classScreenShot.NameResolution = "";
            }
            else
            {
                Debug.Log("<color=red>Разрешение с таким названием уже есть</color>");
            }
        }

        /// <summary>
        /// Добавляем новое разрешение во время создания скриншота
        /// </summary>
        private void AddResolution(string name, int width, int height)
        {
            if (ClassScreenShot.FindSize(ClassScreenShot.GetCurrentGroupType(), name) == -1)
            {
                ClassScreenShot.AddCustomSize(ClassScreenShot.GetCurrentGroupType(),
                    width,
                    height,
                    name);
            }
        }
        /// <summary>
        /// Делаем скрины на разные разрешения с задержкой в пол секунды 
        /// </summary>
        private void TryMakeScreenShot()
        {
            if (classScreenShot != null && classScreenShot.IsActiveScreen)
            {
                if (EditorApplication.timeSinceStartup >= classScreenShot.LastTime + 0.5f)
                {
                    classScreenShot.LastTime = EditorApplication.timeSinceStartup;
                    classScreenShot.CurrentScreenNumber++;

                    if (classScreenShot.M_ClassResolutionScreenShots.Count == 0)
                    {
                        MakeScreenShot();
                        ActiveScreenShot(false);
                    }
                    else if (classScreenShot.CurrentScreenNumber < classScreenShot.M_ClassResolutionScreenShots.Count)
                    {
                        EditorUtility.DisplayProgressBar("Ждём пока доделаются скрины!",
                                                    classScreenShot.CurrentScreenNumber + " / " + classScreenShot.M_ClassResolutionScreenShots.Count,
                                                    (float)classScreenShot.CurrentScreenNumber / classScreenShot.M_ClassResolutionScreenShots.Count);
                        MakeScreenShot();
                    }
                    else
                    {
                        ActiveScreenShot(false);
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
        }

        /// <summary>
        /// Пытаемся сделать скрин
        /// </summary>
        private void MakeScreenShot()
        {
            try
            {
                string filename = "";
                if (classScreenShot.PathFolderForScreenShot == "")
                {
                    SaveScreenShot(filename, classScreenShot.Dir);
                }
                else
                {
                    SaveScreenShot(filename, classScreenShot.PathFolderForScreenShot);
                }
            }
            catch (Exception e)
            {
                Debug.Log("<color=red>Скрин не вышел( </color>" + e.Message);
            }
        }

        /// <summary>
        /// Делаем скриншот
        /// </summary>
        private void SaveScreenShot(string fileName, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log("<color=red>Создаём папку </color>" + path);
            }

            if (classScreenShot.M_ClassResolutionScreenShots.Count > 0)
            {
                if (classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].isActive)
                {
                    AddResolution(classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].NameResolution,
                    classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].Width,
                    classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].Height);
                    ClassScreenShot.SetResolution(classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].NameResolution);
                    fileName = Directory.GetFiles(path).Length +
                    "_" +
                    classScreenShot.M_ClassResolutionScreenShots[classScreenShot.CurrentScreenNumber].NameResolution +
                    ".png";

                    fileName = Path.Combine(path, fileName);
                    //TODO Старый метод, надо дописать для старых версий юнити
                    //Application.CaptureScreenshot(filename, SUPER_SIZE);
                    ScreenCapture.CaptureScreenshot(fileName, classScreenShot.SuperSize);
                    Debug.Log("<color=green>Добавлен скрин </color>" + fileName + "'");
                }
            }
            else
            {
                fileName = Directory.GetFiles(path).Length +
                "_" +
                Application.productName +
                ".png";

                fileName = Path.Combine(path, fileName);
                //TODO Старый метод, надо дописать для старых версий юнити
                //Application.CaptureScreenshot(filename, SUPER_SIZE);
                ScreenCapture.CaptureScreenshot(fileName, classScreenShot.SuperSize);
                Debug.Log("<color=green>Добавлен скрин в текущем разрешении </color>" + fileName + "'");
                Debug.Log("<color=red>В EditorHelper можно добавить неообходимые\n" +
                    "разрешения и делать сразу пачку скринов</color>");
            }

        }

        /// <summary>
        /// Проверяем делять скрины с интерфейсом или без
        /// </summary>
        private void CheckDisableInterface(bool isDisable)
        {
            if (isScreenShotDisableInterface)
            {
                if (isDisable)
                {
                    classScreenShot.UICanvas = null;
                    classScreenShot.UICanvas = FindObjectsOfType<Canvas>();
                }
                ActiveInterface(isDisable);
            }
        }

        /// <summary>
        /// Включаем/отключаем интерфейс
        /// </summary>
        private void ActiveInterface(bool isDisable)
        {
            if (classScreenShot.UICanvas.Length > 0)
            {
                for (int i = 0; i < classScreenShot.UICanvas.Length; i++)
                {
                    classScreenShot.UICanvas[i].enabled = !isDisable;
                }
            }
        }
        #endregion ScreenShotMethods

        /// <summary>
        /// Класс хранящий параметры сцен
        /// </summary>
        [Serializable]
        public class ClassScenes
        {
            public string NameScene = "";
            public string PathScene = "";
            public UnityEngine.Object SceneObject = null;
        }

        #region ClassAutoSave
        /// <summary>
        /// Класс хранящий параметры автосэйва
        /// </summary>
        [Serializable]
        public class ClassAutoSave
        {
            public double LastTime = 0;
            public bool IsActive = false;
            public bool IsActiveGui = true;
            [Tooltip("Tooltip text")]
            public bool IsActiveNotification = true;
            public bool IsActiveNotificationGui = false;
            public int IntervalSave = 10;

            public void SetClassAutoSave(ClassAutoSave classAutoSave)
            {
                EditorPrefs.SetInt(Application.productName + "IntervalAutoSave", classAutoSave.IntervalSave);
                EditorPrefs.SetBool(Application.productName + "IsActiveAutoSave", classAutoSave.IsActive);
                EditorPrefs.SetBool(Application.productName + "IsActiveNotificationAutoSave", classAutoSave.IsActiveNotification);
            }

            public void GetClassAutoSave(ClassAutoSave classAutoSave)
            {
                classAutoSave.IntervalSave = EditorPrefs.GetInt(Application.productName + "IntervalAutoSave", 10);
                classAutoSave.IsActive = EditorPrefs.GetBool(Application.productName + "IsActiveAutoSave", false);
                classAutoSave.IsActiveNotification = EditorPrefs.GetBool(Application.productName + "IsActiveNotificationAutoSave", true);
            }
        }
        #endregion ClassAutoSave

        #region ClassScreenShot
        /// <summary>
        /// Класс хранящий параметры скриншотов
        /// </summary>
        [Serializable]
        public class ClassScreenShot
        {
            public Canvas[] UICanvas = null;
            public bool IsActiveScreen = false;
            public double LastTime = 0;
            public int CurrentScreenNumber = 0;

            public float CurrentTimeScale = 0f;
            public string Dir = "Screenshots";
            //ХЗ почему в скринмейкере стнадартном стоит 2 ?!
            public int SuperSize = 1;
            public UnityEngine.Object FolderForScreenShot = null;
            public string PathFolderForScreenShot = "";
            public List<ClassResolution> M_ClassResolutionScreenShots =
                                        new List<ClassResolution>();
            public string NameResolution = "";
            public int Width = 0;
            public int Height = 0;

            public static MethodInfo getGroup;
            public static object GameViewSizesInstance;

            //Все методы в этом классе честно спизженны и я не до конца понимаю, как всё это работает(
            //Но оно работает и делает скрины 
            static ClassScreenShot()
            {
                //GameViewSizesInstance = ScriptableSingleton<GameViewSizes>.instance;
                var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instanceProp = singleType.GetProperty("instance");
                getGroup = sizesType.GetMethod("GetGroup");
                GameViewSizesInstance = instanceProp.GetValue(null, null);
            }

            public static void AddCustomSize(GameViewSizeGroupType sizeGroupType, int width, int height, string text)
            {
                var group = GetGroup(sizeGroupType);
                var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
                var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
#if UNITY_2017_4_OR_NEWER
                var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                var constructor = gvsType.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });
#else
            var constructor = gameViewSize.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
#endif
                var newSize = constructor.Invoke(new object[] { 1 /*Тип resolution*/, width, height, text });
                addCustomSize.Invoke(group, new object[] { newSize });
            }

            static object GetGroup(GameViewSizeGroupType type)
            {
                return getGroup.Invoke(GameViewSizesInstance, new object[] { (int)type });
            }

            public static void SetResolution(string nameResolution)
            {
                int idx = FindSize(GetCurrentGroupType(), nameResolution);
                if (idx != -1)
                    SetSize(idx);
            }

            public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
            {
                // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
                // string[] texts = group.GetDisplayTexts();
                // for loop...

                var group = GetGroup(sizeGroupType);
                var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
                var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
                for (int i = 0; i < displayTexts.Length; i++)
                {
                    string display = displayTexts[i];
                    // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
                    // so if we're querying a custom size text we substring to only get the name
                    // You could see the outputs by just logging
                    // Debug.Log(display);
                    int pren = display.IndexOf('(');
                    if (pren != -1)
                        display = display.Substring(0, pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
                    if (display == text)
                        return i;
                }
                return -1;
            }

            public static void SetSize(int index)
            {
                var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var gvWnd = EditorWindow.GetWindow(gvWndType);
                selectedSizeIndexProp.SetValue(gvWnd, index, null);
            }

            public static GameViewSizeGroupType GetCurrentGroupType()
            {
                var getCurrentGroupTypeProp = GameViewSizesInstance.GetType().GetProperty("currentGroupType");
                return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(GameViewSizesInstance, null);
            }
        }

        /// <summary>
        /// Класс хранящий параметры скриншотов
        /// </summary>
        [Serializable]
        public class ClassResolution
        {
            public bool isActive = true;
            public string NameResolution = "";
            public int Width = 0;
            public int Height = 0;
        }
        #endregion ClassScreenShot
    }
}
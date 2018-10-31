# -*- coding=utf-8 -*-
import cmdrunner
import os.path
import utils
import buildplug
import logging

pluglist = buildplug.moduleini
plugsfolder = buildplug.modulefolder

gitclone = "git clone --recursive {} {}"
gitreset = "git reset --hard HEAD"
gitpull = "git pull -X theirs"

cwd = os.getcwd()

gitpath = cmdrunner.search_cmd("git")
if not gitpath:
    logging.warning("git not found in path, install it first.")
    exit(0)

cfg = utils.Config(pluglist)
unity_path = cfg.get("DEFAULT", "unity", None)
if not unity_path or not os.path.isfile(unity_path):
    #search unity.exe
    logging.info("Search Unity.exe...") 
    unity_paths = utils.search_file("*/Unity/Editor/Unity.exe")
    if not unity_paths:
        unity_paths = utils.search_file("*/Unity/Hub/Editor/Unity.exe")
    unity_path = unity_paths[0]
if unity_path:
    logging.info("Unity path is: " + unity_path)
    dir_name = os.path.dirname(unity_path)
    unity_assemblies_dir = os.path.join(buildplug.libroot, buildplug.lib_name, buildplug.unity_assemblies)
    #copy unity assemblies to lib folder.
    unityengine_dir = os.path.join(dir_name, "Data/Managed/")
    utils.copy(unityengine_dir + "UnityEditor.dll", unity_assemblies_dir)
    utils.copy(unityengine_dir + "UnityEditor.dll.mdb", unity_assemblies_dir)

    unityengine_dir = os.path.join(unityengine_dir, "UnityEngine/")
    utils.copy(unityengine_dir + "UnityEngine.dll", unity_assemblies_dir)
    utils.copy(unityengine_dir + "UnityEngine.dll.mdb", unity_assemblies_dir)

    unityengine_dir = os.path.join(dir_name, "Data/UnityExtensions/Unity/GUISystem/")
    utils.copy(unityengine_dir + "UnityEngine.UI.dll", unity_assemblies_dir)
    utils.copy(unityengine_dir + "UnityEngine.UI.dll.mdb", unity_assemblies_dir)

#clone plug project.
plugs = buildplug.getusedplugs(cfg)

for plug in plugs:
    plugfoler = os.path.join(plugsfolder, plug)
    logging.info("process plug: [%s] available in config.ini", plug)
    if os.path.isdir(plugfoler): #simple check, todo: upgrade more clever
        if os.path.isdir(os.path.join(plugfoler, ".git")):
            logging.info("git update plug: %s", plug)
            os.chdir(plugfoler)
            cmdrunner.system(gitpull)
            os.chdir(cwd)
    else:
        url = cfg.get(plug, "url")
        if url:
            logging.info("git clone plug: " + plug + " from: " + url)
            cmdrunner.system(gitclone.format(url, plugfoler))
    usedll = utils.str2bool(cfg.get(plug, "usedll"))
    if usedll and os.path.isdir(plugfoler):
        csproj = buildplug.gen_csproj(plug)
        if csproj:
            buildplug.gen_dll(csproj)
            buildplug.gain_dll(plug)
    else:
        logging.info("generate no dll for [usedll=false] or git operation failed on plug: " + plug)





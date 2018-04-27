# -*- coding:utf-8 -*-
import os
import utils
import shutil
import cmdrunner
import fnmatch
import logging

moduleini = "..\\..\\Plugs.ini"
modulefolder = "..\\..\\Plugs"
moduledllfolder = ""
__sharpmake_template= "..\\sharpmake_template\\sharpmake_template\\sharpmake_template.sharpmake.cs"
buildconfig = "Debug"
sharpmake_gen_folder = "generated"
libroot = "..\\.."

libroot = utils.read_config(moduleini, "DEFAULT", "libroot", libroot)

def getusedplugs(cfg: utils.Config):
    all = cfg.fields()
    allset = set(all)
    skips = []
    depends = []
    for one in allset:
        if cfg.getbool(one, "skip"):
            logging.info("skip process plug: " + one)
            skips.append(one)
            continue
        relies = cfg.get(one, "relyon")
        if relies:
            reliess = relies.split(',')
            for rely in reliess:
                relystrip = rely.strip()
                if relystrip == one:
                    logging.warning("dependent plug can't be self: " + one)
                    continue
                if relystrip not in all:
                    logging.warning("dependent plug not exist in plug cfg: " + relystrip)
                    continue
                depends.append(relystrip)
    allset = allset.difference(skips)
    return list(allset.union(depends))




def gen_csproj(plug: str):
    cfg = utils.Config(moduleini)
    usedll = utils.str2bool(cfg.get(plug, "usedll"))
    if not usedll:
        logging.warning("the project is NOT marked using dll, no gen")
        return
    root = cfg.get(plug, "root")
    projname = cfg.get(plug, "projname")
    srcroot = cfg.get(plug, "srcroot")
    if not root or not projname or not srcroot:
        logging.warning("root, projname, srcroot must all exist in cfg.")
        return

    #copy template to root folder.
    root_full = os.path.join(modulefolder, plug, root)
    if not os.path.isdir(root_full):
        logging.warning("plug root folder not exist, check it: %s", root_full)
        return

    ref_dll_list = [cfg.get(plug, x) for x in cfg.options(plug) if x.startswith("refdll_")]
    logging.info("gen csproj for plug: %s, ref dll list: %s", plug, ref_dll_list)
    dest_file = os.path.join(root_full, projname + ".sharpmake.cs")
    shutil.copy(__sharpmake_template, dest_file)
    config_sharp_template(dest_file, plug, cfg)
    #copy lib
    deslibroot = os.path.join(root_full, "Lib")
    for dll in ref_dll_list:
        utils.copy(os.path.join(libroot, dll), deslibroot, libroot)

    sources = '/sources(@"{}")'.format(dest_file)
    sources = sources.replace(os.sep, "/")
    retcode = cmdrunner.system(" ".join(["..\\sharpmake\\Sharpmake.Application.exe", "/verbose", sources]))
    if retcode != 0:
        logging.warning("gen csproj failed: %s", dest_file)
    else:
        gen_folder = os.path.join(root_full, sharpmake_gen_folder)
        for file in os.listdir(gen_folder):
            if fnmatch.fnmatch(file, '*.csproj'):
                return os.path.join(gen_folder, file)


def gen_dll(csproj_path):
    msbuild = utils.read_config(moduleini, "DEFAULT", "msbuild", "..\\..\\")
    msbuild = os.path.join(msbuild, "msbuild.exe")
    if not os.path.isfile(msbuild):
        logging.warning("msbuild not exist at path: %s", msbuild)
        return
    cmdrunner.raw_run([msbuild, csproj_path, "/p:Configuration=" + buildconfig], output=False)


def gain_dll(plug: str, clean=False):
    cfg = utils.Config(moduleini)
    usedll = utils.str2bool(cfg.get(plug, "usedll"))
    if not usedll:
        logging.warning("the project is NOT marked using dll, no gen")
        return
    root = cfg.get(plug, "root")
    projname = cfg.get(plug, "projname")
    srcroot = cfg.get(plug, "srcroot")
    if not root or not projname or not srcroot:
        logging.warning("root, projname, srcroot must all exist in cfg.")
        return
    root_full = os.path.join(modulefolder, plug, root)
    if not os.path.isdir(root_full):
        logging.warning("plug root folder not exist, check it: %s", root_full)
        return
    output_folder = os.path.join(root_full, sharpmake_gen_folder, "output")
    if os.path.exists(output_folder):
        dest = moduledllfolder and moduledllfolder or modulefolder + "Dll"
        logging.info("copy dll from: " + output_folder + " to:" + dest)
        utils.copy(output_folder, dest, output_folder)
        if clean:
            shutil.rmtree(output_folder)
    else:
        logging.warning("no dll output folder found: " + output_folder)


TPL_projName = "[sharpmake.projname]"
TPL_srcRoot = "[sharpmake.srcroot]"
TPL_defines = "[sharpmake.defines]"
def config_sharp_template(template_file: str, plug, cfg: utils.Config):
    projname = cfg.get(plug, "projname")
    srcroot = cfg.get(plug, "srcroot")
    defines = cfg.get(plug, "defines")
    f = open(template_file, "r+")
    filecontent = f.read()
    filecontent = filecontent.replace(TPL_projName, projname, 1)
    filecontent = filecontent.replace(TPL_srcRoot, srcroot, 1)
    filecontent = filecontent.replace(TPL_defines, defines, 1)
    f.seek(0)
    f.truncate()
    f.write(filecontent)
    f.close()


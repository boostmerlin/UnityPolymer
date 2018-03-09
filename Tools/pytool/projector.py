# -*- coding=utf-8 -*-
import argparse
import buildplug
import shutil
import utils
import os
import logging
import glob
import sys

__hostunityprojpath = r"..\..\HostUnityProj"

uniplugfolder = "Plugins"
config = "debug"

def create(args):
    """
    create a project by plugs config
    """
    try:
        logging.info("copy host project...")
        shutil.copytree(__hostunityprojpath, args.pathname)
    except shutil.Error as e:
        logging.error("create project [%s] error:  [%s]", args.pathname, e.args)
        return
    except FileExistsError as e:
        logging.error("target project already exist.")
        return
    logging.info("copy plugs...")
    cfg = utils.Config(buildplug.moduleini)
    plugs = buildplug.getusedplugs(cfg)
    for plug in plugs:
        root = cfg.get(plug, "root")
        projname = cfg.get(plug, "projname")
        srcroot = cfg.get(plug, "srcroot")
        usedll = cfg.getbool(plug, "usedll")
        plugdestfolder = os.path.join(args.pathname, "Assets", uniplugfolder)
        logging.info("copy plug : " + plug)

        ref_dll_list = [cfg.get(plug, x) for x in cfg.options(plug) if x.startswith("refdll_")]
        # copy lib
        for dll in ref_dll_list:
            if "UnityAssemblies" not in dll:
                logging.info("copy ref lib dll: " + dll)
                utils.copy(os.path.join(buildplug.libroot, dll), plugdestfolder, buildplug.libroot)

        if not usedll: # copy sources file.
            plugfoler = os.path.join(buildplug.modulefolder, plug, root, srcroot)
            if not os.path.isdir(plugfoler):
                logging.warning("plug [%s] src files not exist?", plug)
                continue
            exts = cfg.get("DEFAULT", "srcexts", None)
            if exts:
                exts = exts.split(';')
            utils.copy(plugfoler, os.path.join(plugdestfolder, projname), plugfoler, exts)
            #shutil.copytree(plugfoler, os.path.join(plugdestfolder, projname))
        else: # copy dll
            dllfolder = buildplug.modulefolder + "Dll"
            config = args.config
            if not os.path.isdir(dllfolder):
                logging.warning("can't find plug [%s] dll for dll folder not exist.  ", plug)
                continue
            globpattern = os.path.join(dllfolder, "*", config, projname+".dll")
            ret = glob.glob(globpattern)
            if len(ret) == 0:
                logging.warning("can't find plug [%s] dll for dll not exist:%s", plug, globpattern)
                continue
            srcdllfile = ret[0]
            shutil.copy(srcdllfile, plugdestfolder)
            splited_paths = os.path.splitext(srcdllfile)
            srcpdbfile = splited_paths[0] + ".pdb"
            if os.path.exists(srcpdbfile):
                shutil.copy(srcpdbfile, plugdestfolder)
            srcxmlfile = splited_paths[0] + ".xml"
            if os.path.exists(srcxmlfile):
                shutil.copy(srcxmlfile, plugdestfolder)


    logging.info("create project over.")

def compilemodule(args):
    if args.moduleini:
        buildplug.moduleini = args.moduleini
    if args.config:
        buildplug.buildconfig = args.config
    if args.moduledllfolder:
        buildplug.moduledllfolder = args.moduledllfolder
    if args.modulefolder:
        buildplug.modulefolder = args.modulefolder

    clean = utils.str2bool(args.clean)
    cfg = utils.Config(buildplug.moduleini)
    plugs = buildplug.getusedplugs(cfg)
    for plug in plugs:
        usedll = utils.str2bool(cfg.get(plug, "usedll"))
        if not usedll:
            logging.warning("skip compile plug for %s marked not using dll.", plug)
            continue
        csproj = buildplug.gen_csproj(plug)
        if csproj:
            buildplug.gen_dll(csproj)
            buildplug.gain_dll(plug, clean)

parser = argparse.ArgumentParser()
parser.set_defaults(help="sss")
subparsers = parser.add_subparsers(help="projector sub-command", description='valid subcommands')
parser_create = subparsers.add_parser("create", help="create unity project using HostUnityProj")
parser_create.add_argument("--pathname", required=True, help="project destination folder.")
parser_create.add_argument("--config", help="it works when using dll is true in cfg",
                           choices=["Debug", "Release"], default="Debug")
parser_create.set_defaults(func=create)

parser_compile = subparsers.add_parser("compile", help="compile a module by cfg, default using plugs.ini")
parser_compile.add_argument("--modulefolder", help="module root folder.")
parser_compile.add_argument("--moduleini", help="module cfg file, see Plugs.ini.")
parser_compile.add_argument("--moduledllfolder", help="dll folder copied after compile.")
parser_compile.add_argument("--config", help="build config type",
                           choices=["Debug", "Release"], default="Debug")
parser_compile.add_argument("--clean", help="clean build folder. this is not moduledllfolder")
parser_compile.set_defaults(func=compilemodule)
sysargs = sys.argv[1:]
if len(sysargs) == 0:
    sysargs.append("--help")
args = parser.parse_args(sysargs)
args.func(args)

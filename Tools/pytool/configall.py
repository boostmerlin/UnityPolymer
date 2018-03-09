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

#clone project.
cfg = utils.Config(pluglist)
plugs = buildplug.getusedplugs(cfg)

for plug in plugs:
    plugfoler = os.path.join(plugsfolder, plug)
    if os.path.isdir(plugfoler):
        if os.path.isdir(os.path.join(plugfoler, ".git")):
            logging.info("git update plug: %s", plug)
            os.chdir(plugfoler)
            cmdrunner.system(gitpull)
            os.chdir(cwd)
    else:
        url = cfg.get(plug, "url")
        if url:
            logging.info("git clone plug: " + plug + "from: " + url)
            cmdrunner.system(gitclone.format(url, plugfoler))
    usedll = utils.str2bool(cfg.get(plug, "usedll"))
    if usedll and os.path.isdir(plugfoler):
        csproj = buildplug.gen_csproj(plug)
        if csproj:
            buildplug.gen_dll(csproj)
            buildplug.gain_dll(plug)
    else:
        logging.info("gen no dll for config NO [usedll] or git operate failed.")





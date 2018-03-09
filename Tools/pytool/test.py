import cmdrunner
import os
import buildplug

import utils

cfg = utils.Config(buildplug.moduleini)
ret = buildplug.getusedplugs(cfg)
print(ret)
#buildplug.config_sharp_template("sharpmake_template.sharpmake.cs", "protobuf", cfg)

#csproj = buildplug.gen_csproj("UniRx")
#buildplug.gen_dll(csproj)
buildplug.gain_dll("UniRx")

cmdrunner.system("echo ok")
#utils.copy(buildplug.modulefolder + "dll", ".", buildplug.modulefolder + "dll")
# -*- coding:utf-8 -*-

import os
import subprocess
import os.path

ENV_NAME = "MYCMDDIR"


def appdend_dir(path : str):
    mypath = os.getenv(ENV_NAME, ".")
    mypath = ";".join((mypath, path))
    os.putenv(ENV_NAME, mypath)

def search_cmd(name : str):
    """
    :param name: cmd name or full name
    :return: path of the command.
    """
    fileparts = os.path.split(name)
    if fileparts[0] == '': #find in system path
        ret, _ = raw_run(["where", fileparts[1]])
    else:
        ret, _ = raw_run(["where", fileparts[0] + ':' + fileparts[1]])
    if not ret:
        pathpattern = '${}:'.format(ENV_NAME) + fileparts[1]
        ret, _ = raw_run(["where", pathpattern])
    if ret:
        ret = bytes.replace(ret, b'\r\n', b'').decode()
    return ret


def raw_run(cmd : list, output=True, shell=False):
    try:
        print("invoke cmd.. ", cmd)
        stdout = output and subprocess.PIPE or None
        cmdoutput = subprocess.run(cmd, shell=shell, stdout=stdout)
        return cmdoutput.stdout, cmdoutput.returncode
    except subprocess.TimeoutExpired:
        print("call subprocess time out.")


def system(cmd):
    return os.system(cmd)

def run(cmd: list, output=True, shell=False):
    cmd0 = cmd[0]
    ret = search_cmd(cmd0)
    if ret:
        cmd[0] = ret
        out, returncode = raw_run(cmd, output, shell)
        if out:
            return out.decode("gbk"), returncode
        else:
            return None, returncode
    else:
        print("!! cmd", cmd0, "not find.")
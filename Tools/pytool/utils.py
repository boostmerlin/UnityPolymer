# -*- coding:utf-8 -*-
import sys
import os
import configparser
import shutil
import logging
# simple log
logging.basicConfig(format="%(levelname)-8s:%(message)s", level=logging.INFO)
logging.info("current working dir: %s", os.getcwd())


class Config:
    def __init__(self, path):  
        self.path = path
        if not os.path.exists(path):
            open(path, 'w')
        self._cf = configparser.ConfigParser()
        self._cf.read(self.path)

    def get(self, field, key, default=""):
        try:
            result = self._cf.get(field, key, fallback=default)
        except:
            result = default
        return result

    def getbool(self, field, key):
        result = self._cf.getboolean(field, key, fallback=False)
        return result

    def has(self, field, option):
        try:
            v = self._cf[field][option]
            if v:
                return True
        except KeyError:
            pass 

    def set(self, field, key, value):
        try:
            if self._cf.has_section(field):
                self._cf.set(field, key, value)
            else:
                self._cf.add_section(field)
                self._cf.set(field, key, value)
            fp = open(self.path, 'w')
            self._cf.write(fp)
            fp.close()
        except Exception as e:
            print("in config set func", e)
            return False
        return True  

    def remove(self, field, key):
        ret = False
        try:
            ret = self._cf.remove_option(field, key)
        except configparser.NoSectionError:
            print("No section.")
        return ret

    def fields(self):
        return self._cf.sections()

    def options(self, field):
        return self._cf.options(field)

def read_config(config_file_path, field, key, default=""):
    cf = configparser.ConfigParser()
    cf.read(config_file_path)
    result = cf.get(field, key, fallback=default)

    return result


def remove_config(config_file_path, field, key):
    cf = Config(config_file_path)
    return cf.remove(field, key)


def write_config(config_file_path, field, key, value):
    cf = configparser.ConfigParser()
    try:
        if not os.path.exists(config_file_path):
            open(config_file_path, 'w')
        cf.read(config_file_path)

        if cf.has_section(field):
            cf.set(field, key, value)
        else:
            cf.add_section(field)
            cf.set(field, key, value)
        fp = open(config_file_path, 'w')
        cf.write(fp)
        fp.close()
    except Exception as e:
        print(e)
        sys.exit(1)
    return True


def str2bool(s: str):
    return s and s.lower() in ("yes", "1", "true", "on")


def copy(src, dst, root=None, exts: list=None):
    """
    if root is None, copy will flat directory, or the dir structure keeped.
    :param src: src folder
    :param dst: destination folder.
    :param root: root dir, part of src folder.
    :param exts: file ext interest
    """
    relpath = None
    if root:
        relpath = os.path.relpath(src, root)
    try:
        if os.path.isfile(src):
            dstdir = dst
            if relpath:
                dstdir = os.path.join(dst, os.path.dirname(relpath))
            if not os.path.isdir(dstdir):
                os.makedirs(dstdir)
            ext = os.path.splitext(src)[1]
            if not exts or ext in exts:
                shutil.copy(src, dstdir)
        else:
            names = os.listdir(src)
            for name in names:
                srcname = os.path.join(src, name)
                dstname = dst
                if os.path.isdir(srcname):
                    curroot = root
                    if root:
                        dstname = os.path.join(dst, name)
                        curroot = os.path.join(root, name)
                    copy(srcname, dstname, curroot)
                else:
                    copy(srcname, dstname, root)
    except Exception as e:
        print(e)

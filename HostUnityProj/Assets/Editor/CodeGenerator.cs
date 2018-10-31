using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

[Flags]
public enum Language
{
    CSHARP = 1,
    GOLANG = 1 << 1,
    CPP = 1 << 2,
    JAVA = 1 << 3,
    PYTHON = 1 << 4
}

public abstract class CodeGenerator
{
    private TextWriter codeWriter;

    int shrinkLevel = 0;

    /// <summary>
    /// 注释符号//
    /// </summary>
    protected virtual string commentPrefix
    {
        get
        {
            return "//";
        }
    }

    protected virtual string tab
    {
        get
        {
            return "    ";//默认4个空格
        }
    }

    protected virtual string codeBlockBegin
    {
        get
        {
            return "{";
        }
    }

    static Dictionary<Language, string> lang2Ext = new Dictionary<Language, string>()
    {
        { Language.CSHARP, ".cs" },
        { Language.CPP, ".h" },
        { Language.JAVA, ".java" },
        { Language.PYTHON, ".py" },
        { Language.GOLANG, ".go" },
    };

    protected virtual string fileExt
    {
        get
        {
            return lang2Ext[language];
        }
    }

    protected virtual string codeBlockEnd
    {
        get
        {
            return "}";
        }
    }

    protected virtual string nameSpace
    {
        get
        {
            return null;
        }
    }

    protected abstract Language language
    {
        get;
    }

    protected char lineEndingChar
    {
        get
        {
            return '\n';
        }
    }

    protected virtual string separator
    {
        get
        {
            return ";";
        }
    }

    protected virtual void namespaceBegin() { }
    protected virtual void namespaceEnd() { }

    protected void beginCodeblock()
    {
        shrinkLevel++;
    }

    protected void endCodeblock()
    {
        shrinkLevel--;
        if (shrinkLevel < 0)
        {
            Debug.LogError("endCodeblock() invoke more than beginCodeblock()");
            shrinkLevel = 0;
        }
    }

    protected void WriteComment(params string[] commentStr)
    {
        foreach (var comment in commentStr)
        {
            Write(commentPrefix + comment, false);
            lineEnding();
        }
    }

    protected void end()
    {
        if (codeWriter != null)
        {
            codeWriter.Flush();
            codeWriter.Close();
            codeWriter = null;
        }
        if (shrinkLevel > 0)
        {
            Debug.LogError("! beginCodeblock() invoke more than endCodeblock()");
        }
    }

    protected virtual bool begin()
    {
        end();
        if(string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var name = fileName;
        if (!string.IsNullOrEmpty(fileExt) && !name.EndsWith(fileExt))
        {
            name += fileExt;
        }
        FileStream fs = new FileStream(name, FileMode.Create);
        codeWriter = new StreamWriter(fs);

        generateHead();
        //gen license?
        return true;
    }

    /// <summary>
    /// 写一行语句到文件， 不包括换行符
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="withSep">是否添加表达式分格符</param>
    protected void Write(string content, bool withSep = true)
    {
        if (codeWriter != null)
        {
            //行首加tab
            for (int i = 0; i < shrinkLevel; i++)
            {
                codeWriter.Write(tab);
            }
            codeWriter.Write(content);
            if (withSep && !string.IsNullOrEmpty(separator))
            {
                codeWriter.Write(separator);
            }
        }
    }

    /// <summary>
    /// 写一行语句到文件
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="withSep">是否添加表达式分格符</param>
    protected void WriteLine(string content, bool withSep = true)
    {
        if (codeWriter != null)
        {
            //行首加tab
            for (int i = 0; i < shrinkLevel; i++)
            {
                codeWriter.Write(tab);
            }
            if (!string.IsNullOrEmpty(content))
            {
                codeWriter.Write(content);
            }
            if (withSep && !string.IsNullOrEmpty(separator))
            {
                codeWriter.Write(separator);
            }
            lineEnding();
        }
    }

    /// <summary>
    /// 依赖引用的导入
    /// </summary>
    /// <param name="reference"></param>
    protected abstract void import(string reference);

    protected void lineEnding()
    {
        if (codeWriter != null)
        {
            codeWriter.Write(lineEndingChar);
        }
    }

    /// <summary>
    /// full path name of the file, 一般不包括扩展名
    /// </summary>
    public string fileName { get; set; }

    /// <summary>
    /// true, 不执行
    /// </summary>
    public bool Omit { get; set; }

    protected virtual void generateHead()
    {
        //提示
        string timeStr = System.DateTime.Now.ToString();
        WriteComment("This code was auto generated, don't edit it.",
            "Time: " + timeStr);

        WriteLine("");
        WriteLine("");
    }

    /// <summary>
    /// 重写以导入需要的依赖
    /// </summary>
    protected virtual void importReferences() { }

    /// <summary>
    /// 生成代码主体
    /// </summary>
    protected virtual void generateBody() { }

    public virtual void Run()
    {
        if (!Omit && begin())
        {
            importReferences();
            namespaceBegin();
            generateBody();
            namespaceEnd();
            end();
        }
    }
}


public abstract class CSharpGenerator : CodeGenerator
{
    protected override Language language
    {
        get
        {
            return Language.CSHARP;
        }
    }

    protected override void namespaceBegin()
    {
        if (!string.IsNullOrEmpty(nameSpace))
        {
            WriteLine("namespace " + nameSpace + " " + codeBlockBegin, false);
        }
    }

    protected override void namespaceEnd()
    {
        if (!string.IsNullOrEmpty(nameSpace))
        {
            WriteLine(codeBlockEnd, false);
        }
    }

    protected override void import(string reference)
    {
        WriteLine("using " + reference);
    }
}

public class PythonGenerator : CodeGenerator
{
    protected override Language language
    {
        get
        {
            return Language.PYTHON;
        }
    }

    protected override string separator
    {
        get
        {
            return "";
        }
    }

    protected override string commentPrefix
    {
        get
        {
            return "#";
        }
    }

    protected override void import(string reference)
    {
        WriteLine("import " + reference);
    }

    protected void import(string reference1, string reference2)
    {
        WriteLine("from " + reference1 + " import " + reference2);
    }
}

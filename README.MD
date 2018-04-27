### 方便管理和整合常用的框架，工具。也会把一些开发中的积累放在这里
-------------
#### 必要环境：
* 安装python3.5+ ？
* 把UnityPolymer\Tools\bin 放到path环境变量里
* 安装vs2017
* ploymer.bat init 获取Plugs.ini中配置的插件，将放置于Plugs文件夹下，也可以手动拷贝到该目录下配置
* Lib 文件夹放置编译后或者手动拷贝的库文件
> sharpmake是一个生成vs工程等的工具，见：https://github.com/ubisoftinc/Sharpmake


#### Plugs.ini 配置文件：
*该文件用来管理，编译插件*  
;默认值  
[DEFAULT]  
;msbuild 安装位置  
msbuild=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin  
;默认的 lib root is ..\.., 即git根目录  
;libroot=..\..  
;拷贝文件时的扩展名  
;srcexts=.cs

;一个插件节点  
[protobuf]  
;git src url. clone 之后的根目录名为节点名.  
url=https://github.com/google/protobuf.git  
;sharpmake 文件放这里. 相对于插件目录：i.g. Plugs\protobuf\[csharp\src]  
root=csharp\src  
;sharpmake 生成的文件名  
projname=Google.Protobuf  
;插件源码根目录, 相对于root目录, 如果 usedll=false,  这个目录下的源码会拷贝到unity工程
srcroot=Google.Protobuf  
;是否使用dll还是源码  
usedll=true  
;编译dll时的预编译宏  
defines=NET35  
;依赖插件，即使skip=false，也会使用  
;relyon=UniRx,protobuf  
;是否跳过处理这个节点的插件  
skip=false  

* ploymer.bat create 创建unity工程，初始目录位于：Tools\pytool
> hostunityproj使用的是unity 2017版本

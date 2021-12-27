# 启动器
首先安装mirai-console-loader 你可以选择手动安装或者下载器安装
#### 1.手动安装
1.  下载[mirai-console-loader](https://github.com/iTXTech/mirai-console-loader) Releases包 
2.  安装 Java 运行时（版本必须 >= 11）
3.  从 Releases 下载最新版本的MCL并解压到某处

#### 2.下载器安装
1.  下载[mcl-installer](https://github.com/iTXTech/mcl-installer)Releases包 
2.  运行 mcl-installer安装文件

#### 启动mcl
1.  在安装后的文件目录下运行cmd
2.  在命令行中执行.\mcl以启动MCL

#### 配置mirai-api-http
1.  手动下载[mirai-api-http](https://github.com/project-mirai/mirai-api-http)
2.  将下载好的jar文件放到 mcl的plugins文件夹下
3.  重新执行.\mcl以重启动MCL

#### 登录机器人账号
在命令行中输入
```
login QQ号 密码 
```
#### 配置mirai-api-http端口与token
1.  打开\config\net.mamoe.mirai-api-http\setting.yml文件
2.  port为端口 authKey为密钥

# 机器人配置
```
{
  "QQ": "2960029899",//mcl里登录的QQ
  "Address": "127.0.0.1:1302",//mirai-api-http开放的地址
  "Key": "key"//mirai-api-http的authKey
}
```
# 作者
QQ1332502800,部署时不清楚怎么办可以问我
## 致谢

- [mirai](https://github.com/mamoe/mirai)
- [mirai-api-http](https://github.com/project-mirai/mirai-api-http)
- [Mirai.NET](https://github.com/SinoAHpx/Mirai.Net) 
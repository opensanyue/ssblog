# 山水博客 （ssblog）v0.0.1

> “山水博客”是基于.net8的博客系统 ，可以部署到win10自带的iis上，详细参看项目根目录下的“山水博客安装教程”。
>
> 本教程主要介绍博客的使用





![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/1.png)





##  一、界面简介

如上图，左侧“分类”，右侧“文章列表”。

选中“分类”的“`全部显示`”会显示所有文章列表。分类旁的数字“`101`”是全部文章`总数`。

除“`全部显示`”以外的分类，如后面`有数字`，则表示其下文章的总数（`不包含其下子分类下的文章`）。

点击右上角的“`登陆`”:

* 默认账号：ssblog@163.com

* 默认账号: 11111111

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/2.png)

`账号一定要是邮箱格式`。



## 二、管理分类

登陆后，我们可以添加编辑`分类`和`文章`了。

选中“全部显示”，在分类标题栏上单击“添加”，可以追加一级分类。

如果选中其它分类，”添加“会追加为子分类。

单击”编辑“，每条分类后会有3个按钮。

* 第1个：当前分类之后插入同级分类。
* 第2个：编辑分类名称。
* 第3个：删除分类。同时会删除分类下的文章及子分类。 
* 移动位置。按住想移动的”分类“，拖到目标分类之前、之后、之上，实现插入到之前，之后和成为目标分类的子分类。

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/3.png)





## 三、管理文章

本博客支持Markdown和Html两种文章格式，在”文章列表"的"详情"中会显示文章的格式。

点击”编辑“会根据格式自动调用相应的编辑器来编辑。

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/4.png)



在”文章列表“标题栏，单击”添加MD“和“添加”来添加文章，

Markdown编辑器：

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/5.png)

Html编辑器

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/6.png)





## 三、文章排序

每一个分类，都可以设置一种排序。

我们选中一个分类，单击“文章列表”标题栏最右侧的“三个点”，就可以选择排序了。

默认排序：按更新日期，倒序。

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/7.png)

如果我们想实现下面这样的排序呢。

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/8.png)

首先，你要在你的标题里，像上图一样插入数字，且每个标题“数字”之前的``字符要一样``。

然后，选择“按标题名” 和 “正序”。





## 四、导入MarkDown(Typoar)

* 选中准备导入博客的分类

* 单击“文章列表”标题栏右上角的“三点”，选“导入MarkDown(Typoar)”

  ![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/9.png)

如果只导入一篇博客，把博客路径和博客名输入到第一行，如果博客有图片，在第二行输入图片放置的路径，不包含"image"

如果导入多篇博客，只要把博客所在路径输入到第一行(如上图)，再设置图片的路径（上图中图片的真实路径是”e:\我的文档\Markdown\images“，这意味着你的图片也要放到名为“images”的目录下）。





## 五、修改密码

为了安全请尽快修改密码，单击主页右上角的用户名。

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/10.png)





## 六、修改“网站名“

单击点“系统设置”

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/11.png)





## 七、备份和恢复

在网站安装根目录下，复制"upload"目录和db.db数据库（如有db.db-shm和db.db-wal也要复制）到保存位置即可。

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/12.png)

最好可以打包成rar压缩文件，方便管理

![](https://github.com/opensanyue/ssblog/blob/master/aspnetMCVBSUser1/imgs/13.png)

恢复时，只需将备份的文件复制回去即可（恢复前，请关闭打开的网页，`最好重启后，再恢复`）。

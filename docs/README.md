## SSO SDK for CSharp

#### 状态代码含义

* 服务状态：0开发中 1审核中 2审核通过 3未通过审核 4已下线 5已存档
* 表单状态：0填表中 1签批中 2等待受理 3受理通过 4受理驳回 5驳回重填 9已删除
* 表单audit状态：0等待1通过2驳回3需我签批4别人已签批
* 推荐工作环境 Visual Studio 2013


### 用户管理

* url: /account
* 请求方式：POST
* 数据格式：json
    + Headers

             Authorization: <oauth 2.0 token>

    + Body

            {
                "name": "sample user name"
                "password": "password"
            }



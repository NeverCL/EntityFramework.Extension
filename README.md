# [高并发]EntityFramework之高性能扩展

## 目录
- [简介](#简介)
- [读写分离](#读写分离)
- [指定字段更新](#指定字段更新)
- [事务](#事务)
- [Entity](#entity)

## 简介
- 本EF扩展插件将持续更新：开源，敏捷，高性能。(由于EF Core暂未提供方便的钩子位置，暂无EF Core版本)

- [EntityFramework.Extension代码](https://github.com/NeverCL/EntityFramework.Extension) (GitHub欢迎Fork)

- [EntityFramework.Extension代码](https://www.nuget.org/packages/EntityFramework.Extension/) (Nuget：Install-Package EntityFramework.Extension)

## 读写分离
读写分离，支持可配置项的方式。同时支持权重的方式轮询。

- 先看段配置文件
```xml
 <entityFrameworkConfig isSlaveRead="true" readConnstr="Data Source=(localdb)\test;Initial Catalog=Demo;Integrated Security=True;">
    <slaves>
      <add name="test1" connectionString="Data Source=(localdb)\test;Initial Catalog=Demo;Integrated Security=True;" weight="1"/>
      <add name="test2" connectionString="Data Source=(localdb)\test;Initial Catalog=Demo;Integrated Security=True;" weight="10"/>
    </slaves>
  </entityFrameworkConfig>
```
- `isSlaveRead`   // 是否开启读写分离
- `readConnstr`   // 读库链接字符串
- `slaves节点`    // 当读库有多个时，通过`weight`支持权重轮询读库功能。(readConnstr配置不为空时，将忽略slaves节点)

## 指定字段更新
目前封装了3种形式的，指定字段更新方法。

- 对象不存在上下文
```c#
var user = new User { Id = 2, Name = Guid.NewGuid().ToString() };
DemoDbContext.CurrentDb.UpdateField(user, "Name");
```

- 对象已存在上下文
```c#
var user = new User { Id = 2, Name = Guid.NewGuid().ToString() };
DemoDbContext.CurrentDb.UpdateField(user, x => x.Id == 2, "Name");
```

- 对象为IEntity,无论是否存在上下文均支持
```c#
var user = new User { Id = 2, Name = Guid.NewGuid().ToString() };
DemoDbContext.CurrentDb.UpdateEntityField(user, "Name");
```
## 事务
- 事务类型
在.NET 中，事务分SQLTransaction和TransactionScope。后者在MSDTC(Distributed Transaction Coordinator)开启的时候，支持分布式事务。
    - TransactionScopeOption
        - Required
            - 默认方式，如果存在环境事务，直接取环境事务，如果不存在，则创建新的
        - RequiresNew
            - 直接创建新的环境事务
        - Suppress
            - 取消当前区域环境事务

- 隔离等级IsolationLevel
    - ReadUncommitted(读未提交)
        - 表示：未提交读。当事务A更新某条数据的时候，不容许其他事务来更新该数据，但可以进行读取操作
    - ReadCommitted(读提交)
        - 表示：提交读。当事务A更新数据时，不容许其他事务进行任何的操作包括读取，但事务A读取时，其他事务可以进行读取、更新
    - RepeatableRead
        - 表示：重复读。当事务A更新数据时，不容许其他事务进行任何的操作，但是当事务A进行读取的时候，其他事务只能读取，不能更新
    - Serializable
        - 表示：序列化。最严格的隔离级别，当然并发性也是最差的，事务必须依次进行。
    - 默认级别
        - Oracle	read committed
        - SqlServer	read committed
        - MySQL(InnoDB)	Read-Repeatable

- 事务特性(ACID)
    - 原子性（Atomicity）
        - 事务是数据库的逻辑工作单位，事务中的诸多操作要么全做要么全不做
    - 一致性（Consistency）
        - 事务执行结果必须是使数据库从一个一致性状态变到另一个一致性状态
    - 隔离性（Isolation）
        - 一个数据的执行不能被其他事务干扰
    - 持续性/永久性（Durability）
        - 一个事务一旦提交，它对数据库中的数据改变是永久性的

说了那么多，本插件对事务的支持：

```c#
DemoDbContext.CurrentDb.TransExecute(x => {
    x.Users.Add(new User());
    return x.SaveChanges();
});
```

针对事务，同时支持锁的读取功能
```c#
var userList = DemoDbContext.CurrentDb.NoLockFunc(db => db.Users.ToList());
```

## Entity
类似ABP框架，提供了IEntity，ICreatorEntity，IModifyEntity，IAuditionEntity，IDeletionEntity等等
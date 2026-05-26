# AttackTrigger 使用说明

## 功能概述

这个系统允许玩家与两个攻击触发器交互：
1. 玩家靠近时显示"按E激活"提示
2. 按E键激活触发器（从未激活变为已激活状态）
3. 当两个触发器都激活时，它们之间会自动连成一条线

## 脚本说明

### 1. AttackTrigger.cs（单个触发器脚本）
挂载在每个攻击触发器对象上的脚本。

### 2. AttackTriggerManager.cs（管理器脚本）
负责管理所有触发器并绘制连线的管理器。

## 使用步骤

### 第一步：创建AttackTriggerManager
1. 在场景中创建一个空GameObject，命名为"AttackTriggerManager"
2. 添加`AttackTriggerManager`组件
3. 在Inspector中配置：
   - **Line Material**: 为连线指定材质（可以使用默认材质或创建新的）
   - **Line Width**: 连线宽度（默认0.1）
   - **Line Color**: 连线颜色（默认黄色）

### 第二步：创建AttackTrigger对象
1. 创建两个GameObject作为攻击触发器（可以用Sphere、Cube等）
2. 为每个对象添加`AttackTrigger`组件
3. 添加Collider组件（确保勾选"Is Trigger"选项）
4. 在Inspector中配置每个AttackTrigger：
   - **Overhead Text**: 拖入一个TextMeshProUGUI组件用于显示提示
   - **Text Height Offset**: 提示文字的高度偏移
   - **Visual Marker**: 触发器的视觉标记对象（通常就是触发器自己）
   - **Inactive Color**: 未激活时的颜色（默认红色）
   - **Active Color**: 激活后的颜色（默认绿色）

### 第三步：创建UI提示文本
1. 在Canvas下创建一个TextMeshPro - Text(UI)对象
2. 设置文字内容为"按E激活"
3. 调整合适的字体大小和颜色
4. 将这个TextMeshProUGUI拖到每个AttackTrigger的**Overhead Text**字段中

### 第四步：设置玩家
确保玩家对象有"Player"标签，并且有Collider组件。

## 工作流程

1. 玩家靠近第一个AttackTrigger → 显示"按E激活"提示
2. 玩家按E键 → 第一个触发器激活（颜色改变）
3. 玩家靠近第二个AttackTrigger → 显示提示
4. 玩家按E键 → 第二个触发器激活
5. 两个触发器之间自动出现连线！

## 注意事项

- 确保触发器的Collider勾选了"Is Trigger"
- 确保玩家有"Player"标签
- Line Material可以使用Unity内置的"Default-Line"材质，或者自己创建

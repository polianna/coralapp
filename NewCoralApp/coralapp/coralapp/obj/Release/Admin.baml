   M S B A M L   `   `   `  ���� C  ?coralapp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:coralappcoralapp      coralapp.Admin   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:coralapp       � Title$C  =Склад магазина здорового питания�����	#FFC5FFFF � 
ResizeMode$ CanResizeWithGrip=�$	��800��$	��600��.��� $��6,0,0,-4q� � RowDefinitions �� $	��67* ��� $
��430* �.+��� -    ��Name$ lLogo��$��
Склад��$
��Left=�$��0,5,0,0q�$	��Top=�$��36��$��96��$��Segoe Script)�$��24(�$
��Bold"����	#FF073234�� -   $ lDesc��$6��0Магазин здорового питания��$
��Left=�$��53��$	��Top=�$	��240��$��Segoe Script)�$��0,41,0,0q�$��14(����	#FF073234$
��Bold"�$��2���� -    ��Name$ 
tabControl��$��1��$��0,0,5,10q�
�������.@��� -    ��Name$ tabLogs��$,��&Журнал учёта событий��
�������$��Comic Sans MS)�.��� 
�������$��0,12,0,0q�.+��� -   $ 	lUserName��$��Пользователь��$
��Left=�$��34��$��	10,-3,0,0q�$	��Top=�$	��110��$��14(��� -    ��Name$ 
cbUserName��$
��Left=�$��34��$��	125,0,0,0q�$	��Top=�$	��178�� w� TextPath$ username�� �� DisplayMemberPath$ username��	 �� IsTextSearchEnabled	 . 
 �� 
IsEditable
 . ����  �� SelectedValue �� *
username+�� -    ��Name$ bAllLog��$��Снять фильтр��$
��Left=�$��34��$��
308,39,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF�� -   $ 
bFilterLog��$ ��Отфильтровать��$
��Left=�$��34��$��	308,0,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF&   System.Windows.Controls.DataGrid  -	     Name$ dgLog��$��	5,80,5,10q�   AutoGenerateColumns .     Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   
IsReadOnly .    Header$ Дата события��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 120 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *
insertdate+   . $ Пользователь��$	 120  �� *
username+   . $ Событие��$	 300  �� *	logtext+�� -
   $ 	lFromDate��$��c��$
��Left=�$��34��$��	10,35,0,0q�$	��Top=�$��25��$��14(� J� RenderTransformOrigin$ 0.266,0.326&�(  "System.Windows.Controls.DatePicker  -     Name$ 
dpFromDate��$
��Left=�$��	40,38,0,0q�$	��Top=�$	��112��   FirstDayOfWeek$ Monday=�$��34���� -   $ lToDate��$
��по��$
��Left=�$��34��$��
157,35,0,0q�$	��Top=�$��29��$��14(�$ -1.293,1.705&�  -   $ dpToDate��$
��Left=�$��
191,38,0,0q�$	��Top=�$	��112��$ Monday=�$��34���� -   $ tabUsers��$7��1Управление пользователями��
�������$��Comic Sans MS)�.��� 
�������$��0,12,0,0q�.+��� -   $ lName��$��Пользователь��$
��Left=�$��34��$��10,0,0,0q�$	��Top=�$	��110��$��14(��� -    ��Name$ 
tbUserName��$
��Left=�$��34��$��	125,0,0,0q� �� TextWrapping$
 Wrap=�$�� ��$	��Top=�$	��120���� -   $ 	lPassword��$��Пароль��$
��Left=�$��	10,39,0,0q�$	��Top=�$��14(��� -   $ lActive��$��Активный��$
��Left=�$��	250,0,0,0q�$	��Top=�$ 1.731,0.344&�$��14(��� -   $ tbDescription��$
��Left=�$��34��$��	456,0,0,0q�$
 Wrap=�$�� ��$	��Top=�$	��120���� -   $ lDescription��$��Описание��$
��Left=�$��	373,0,0,0q�$	��Top=�$��14(��� -   $ lRole��$��Роль��$
��Left=�$��
250,39,0,0q�$	��Top=�$��14(��� -   $ cbRole��$
��Left=�$��
297,39,0,0q�$	��Top=�$	��120��$��34��$ 	role_name��	 . 
 . $ �� IsSynchronizedWithCurrentItem$
 TrueY�  -   $ dgUser��$��	5,78,5,10q� .      . $ Пользователь��$	 120  �� *
username+   . $ Активный��$ 70  �� *
isactive+   . $ Описание��$	 120  �� *desc+   . $ Роль��$	 120  �� *	role_name+   . $ Пароль��$	 120  �� *pass+   . $! Последний вход��$	 120  �� *	lastlogin+   . $ Создан��$	 120  �� *
insertdate+   . $ Обновлен��$	 120  �� *
updatedate+�� -   $ 	bSaveUser��$��Сохранить��$
��Left=�$��34��$��	581,0,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF�� -   $ bCancelUser��$��Отменить��$
��Left=�$��34��$��
581,39,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFFG� -    G�Name$ 
pbPassword��$
��Left=�$��
125,39,0,0q�$	��Top=�$	��120��$��34���� -    ��Name$ cbActive��$
��Да��$
��Left=�$��	333,5,0,0q�$	��Top=�$��14(�$ 1.729,3.875&��� -   $ tabDb��$>��8Изменение данных в базе данных��
�������$��Comic Sans MS)�.��� 
�������$��0,12,0,0q�.+��� -   $ 
lTableName��$��Таблица��$
��Left=�$��34��$��5,0,0,0q�$	��Top=�$	��110��$��14(��� -   $ cbTableName��$
��Left=�$��34��$��	120,0,0,0q�$	��Top=�$	��296��$ 
table_name��$ 
table_name��	 . 
 . ����  �� *
table_name+�� -   $ lSQLText��$��SQL Запрос��$
��Left=�$��34��$��5,46,0,0q�$	��Top=�$	��110��$��14(�$ 0.501,2.122&��� -    $ 	tbSQLText��$
��Left=�$��
120,53,0,0q�$
 Wrap=�$�� ��$	��Top=�$��68��$	��457���� -!   $ 
bTableView��$%��Просмотр таблицы��$
��Left=�$��34��$��	421,0,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF�� -"   $ bSQLExec��$��Выполнить SQL��$
��Left=�$��34��$��
582,87,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF  -#   $ dgTable��$��
5,126,5,46q� . �� -$   $ bSave��$��Сохранить��$��Right=�$��34��$��0,0,5,5q�$��Bottom=�$	��131��$��14(����	#FFC5FFFF{�.  �� -%   $ bCancel��$��Отменить��$��Right=�$��34��$��	0,0,150,5q�$��Bottom=�$	��131��$��14(����	#FFC5FFFF{�.  �� -&   $ bClose��$��
Выйти��$��Right=�$��34��$��	0,15,13,0q�$	��Top=�$��91��$��14(�
�������
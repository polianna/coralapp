   M S B A M L   `   `   `  ���� C  ?coralapp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:coralappcoralapp      coralapp.manager   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:coralapp       � Title$C  =Склад магазина здорового питания�����	#FFC5FFFF � 
ResizeMode$ CanResizeWithGrip=�$	��800��$	��600��.��� $��6,0,0,-4q� � RowDefinitions �� $	��67* ��� $
��430* �.+��� -    ��Name$ lLogo��$��
Склад��$
��Left=�$��0,5,0,0q�$	��Top=�$��36��$��96��$��Segoe Script)�$��24(�$
��Bold"����	#FF073234�� -   $ lDesc��$6��0Магазин здорового питания��$
��Left=�$��53��$	��Top=�$	��240��$��Segoe Script)�$��0,41,0,0q�$��14(����	#FF073234$
��Bold"�$��2���� -    ��Name$ 
tabControl��$��1��$��0,0,5,10q�
�������.@��� -    ��Name$ tabDashboard��$'��!Панель управления��
�������$��Comic Sans MS)�.��� 
�������$��0,2,0,-2q��� -   $ tabList��$!��Списки товаров��
�������$��Comic Sans MS)�.��� 
�������$��0,2,0,-2q�.+��� -    ��Name$ bListCriticalAmount��$@��:Список заканчивающихся товаров��$��34��$��	10,10,0,0q�$	��Top=�$	��246��$��14(����	#FFC5FFFF$
��Left=��� -   $ bListPromoProducts��$4��.Список акционных товаров��$��34��$��
261,10,0,0q�$	��Top=�$	��246��$��14(����	#FFC5FFFF$
��Left=��� -   $ bListBuy��$4��.Составить список покупок��$��34��$��
512,10,0,0q�$	��Top=�$	��246��$��14(����	#FFC5FFFF$
��Left=�&   System.Windows.Controls.DataGrid  -	     Name$ dgList��$��
10,61,0,10q�   AutoGenerateColumns .  	   Columns	 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn
   
IsReadOnly
 .    Header$+ %Наименование товара��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 250 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *name+  
 . $ Код товара��$	 120  �� *	coralid+  $' !Количество товара��$	 120  �� *
quantity+�� -
   $ tabSale��$/��)Инициализация закупки��
�������$��Comic Sans MS)�.��� 
�������$��0,2,0,-2q�.+��� -   $ lNewProductName��$��Наименование��$
��Left=�$��34��$��	10,-1,0,0q�$	��Top=�$	��110��$��14(��� -    ��Name$ cbNewProductName��$
��Left=�$��34��$��
125,-1,0,0q�$	��Top=�$	��130���� -   $ lNewProductCode��$��Код товара��$
��Left=�$��34��$��
260,-1,0,0q�$	��Top=�$��86��$��14(��� -   $ cbNewProductCode��$
��Left=�$��34��$��	351,0,0,0q�$	��Top=�$	��131���� -   $ lNewProductQuantity��$��Количество��$
��Left=�$��34��$��
487,-1,0,0q�$	��Top=�$��89��$��14(��� -    ��Name$ tbNewProductQuantity��$��0��$
��Left=�$��34��$��	583,3,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$��41���� -   $ bNewAddInTable��$��Добавить��$
��Left=�$��34��$��	629,3,0,0q�$	��Top=�$	��130��$��14(����	#FFC5FFFF  -   $ dgNew��$��	5,42,5,45q� .  	   
 . $+ %Наименование товара��$	 250  �� *name+  
 . $ Код товара��$	 120  �� *	coralid+  $' !Количество товара��$	 120  �� *
quantity+�� -   $ bNewAddInDB��$>��8Экспортировать список покупок��$��Right=�$��34��$��0,0,5,5q�$��Bottom=�$	��234��$��14(����	#FFC5FFFF�� -   $ tabForecast��$��Прогнозы��
�������$��Comic Sans MS)�.��� 
�������$��0,2,0,-2q�.+��� -   $ lForecastDisc��$b��\Здесь можно посмотреть прогноз на следующий месяц��$
��Left=�$��34��$��10,0,0,0q�$	��Top=�$	��710��$��14(��� -   $ bForecastSale��$=��7Построить прогноз по продажам��$
��Left=�$��34��$��	10,34,0,0q�$	��Top=�$	��236��$��14(����	#FFC5FFFF�� -   $ bForecastLedger��$=��7Построить прогноз по остаткам��$
��Left=�$��34��$��
264,34,0,0q�$	��Top=�$	��236��$��14(����	#FFC5FFFF�� -   $ bClose��$��
Выйти��$��Right=�$��34��$��	0,15,13,0q�$	��Top=�$��91��$��14(�
�������
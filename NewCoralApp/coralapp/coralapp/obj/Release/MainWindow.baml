   M S B A M L   `   `   `  ���� C  ?coralapp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:coralappcoralapp      coralapp.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:coralapp       � Title$C  =Склад магазина здорового питания�����	#FFC5FFFF � 
ResizeMode$ CanResizeWithGrip=�$	��800��$	��600�� � 	Resources    	eTemplate �� ElementName �� Path ��Name��%�   &	        �� .R�]� ��. .+��� $��Right=�
����  �$��13(����� $ adorned��$8 2AdornedElement.(Validation.Errors)[0].ErrorContent��� 
���  ��$��2q�._��� $ adorned��.��� $��6,0,0,-4q� � RowDefinitions �� $	��67* ��� $
��430* �.+��� -    ��Name$ lLogo��$��
Склад��$
��Left=�$��0,5,0,0q�$	��Top=�$��36��$��96��$��Segoe Script)�$��24(�$
��Bold"����	#FF073234�� -   $ lDesc��$6��0Магазин здорового питания��$
��Left=�$��53��$	��Top=�$	��240��$��Segoe Script)�$��0,41,0,0q�$��14(����	#FF073234$
��Bold"�$��2���� -    ��Name$ 
tabControl��$��1��$��0,0,5,10q�
�������	 �� SelectedIndex$	 0��.@��� -   
 ��Name$
 tabSale��$!��Продажа товара��
�������$��Comic Sans MS)�.��� 
�������$��0,12,0,0q�.+��� -   $ lSaleProductName��$��Наименование��$��34��$��49,0,0,0q�$	��Top=�$	��110��$��14(�$
��Left=��� -    ��Name$ cbSaleProductName��$��34��$��	165,0,0,0q�$	��Top=�$	��154��$
��Left=� w� TextPath$ commodity_name�� �� DisplayMemberPath$ commodity_name�� �� IsTextSearchEnabled .  �� 
IsEditable . ����  �� SelectedValue �� *
price_id+�� -   $ lSaleProductCode��$��Код товара��$��34��$��	325,0,0,0q�$	��Top=�$��90��$��14(�$
��Left=��� -   $ cbSaleProductCode��$��34��$��	419,0,0,0q�$	��Top=�$	��156��$
��Left=�$ coralclub_id��$ coralclub_id�� .  . ����  �� *
price_id+�� -	   $ lSaleProductQuantity��$��Количество��$��34��$��	49,39,0,0q�$	��Top=�$	��110��$��14(�$
��Left=��� -
    ��Name$ tbSaleProductQuantity��$��34��$��
165,39,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��154��$
��Left=�$��0���� -    ��Name$ cbSalePromo��$��Акция?��$��34��$��
337,39,0,0q�$	��Top=�$��78��$��14(�$
��Left=��� -    ��Name$ bSaleAddInTable��$(��"Добавить в корзину��$��34��$��
419,39,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF$
��Left=�&   System.Windows.Controls.DataGrid  -     Name$ dgSale��$��	5,80,5,45q�   AutoGenerateColumns .     Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   
IsReadOnly .    Header$+ %Наименование товара��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 200 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *name+   . $ Код товара��$	 120  �� *	coralid+  $ По акции 1��$ 80  �� *	onSale1+  $ По акции 2��$ 80  �� *	onSale2+  $ По акции 3��$ 80  �� *	onSale3+  $ Без акции��$ 80  �� *withoutSale+   . $ 
Итого��$ 80  �� *
quantity+�� -   $ bSaleAddInDB��$��Продать��$��34��$��0,0,5,5q�$��Bottom=�$	��156��$��14(����	#FFC5FFFF$��Right=��� -   $
 tabNew��$'��!Новые поступления��
�������$��Comic Sans MS)�$��Right=�$	��135��.��� 
�������$��0,12,0,0q�.+��� -   $ lNewProductName��$��Наименование��$
��Left=�$��34��$��50,0,0,0q�$	��Top=�$	��110��$��14(��� -   $ cbNewProductName��$
��Left=�$��34��$��	166,0,0,0q�$	��Top=�$	��154��$ commodity_name��$ commodity_name�� .  . ����  �� *
price_id+�� -   $ lNewProductCode��$��Код товара��$
��Left=�$��34��$��	326,0,0,0q�$	��Top=�$��90��$��14(��� -   $ cbNewProductCode��$
��Left=�$��34��$��	420,0,0,0q�$	��Top=�$	��156��$ coralclub_id��$ coralclub_id�� .  . ����  �� *
price_id+�� -   $ lNewProductQuantity��$��Количество��$
��Left=�$��34��$��	50,38,0,0q�$	��Top=�$	��110��$��14(��� -   $ tbNewProductQuantity��$��0��$
��Left=�$��34��$��
166,38,0,0q�$
 Wrap=�$	��Top=�$	��154���� -   $ bNewAddInTable��$��Добавить��$
��Left=�$��34��$��
597,38,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF�� -   $ bNewUploadFromExcel��$$��Загрузить из excel ��$
��Left=�$��34��$��	597,0,0,0q�$	��Top=�$	��156��$��14(����	#FFC5FFFF  -   $ dgNew��$��	5,80,5,45q� .      . $+ %Наименование товара��$	 250  �� *name+   . $ Код товара��$	 120  �� *	coralid+  $' !Количество товара��$	 120  �� *
quantity+   . $ Срок годности��$	 120  �� *expirationdate+�� -   $ bNewAddInDB��$/��)Добавить в базу данных��$��Right=�$��34��$��0,0,5,5q�$��Bottom=�$	��176��$��14(����	#FFC5FFFF�� -   $ lNewProductDate��$��Годен до��$
��Left=�$��34��$��
326,38,0,0q�$	��Top=�$��90��$��14(�(  "System.Windows.Controls.DatePicker  -     Name$ dpExpirationDate��$
��Left=�$��
420,41,0,0q�$	��Top=�$	��156��   FirstDayOfWeek$ Monday=�$��31���� -   $
 	tabSearch��$��Что на складе?��
�������$��Comic Sans MS)�.��� 
�������$��0,12,0,0q�.+��� -   $ bSearchAllProduct��$+��%Весь товар на складе��$
��Left=�$��34��$��5,0,0,5q�$��Bottom=�$	��298��$��14(����	#FFC5FFFF�� -   $ bSearchPromoProduct��$5��/Акционный товар на складе��$��Right=�$��34��$��0,0,5,5q�$��Bottom=�$	��298��$��14(����	#FFC5FFFF�� -   $ tbSearchProductName��$
��Left=�$��34��$��	380,0,0,0q�$
 Wrap=�$	��116��$��наименование��$��14(�$	��Top=��� -    $ bSearchProduct��$��Поиск товара��$
��Left=�$��34��$��	657,0,0,0q�$	��Top=�$	��104��$��14(����	#FFC5FFFF�� -!   $ lSearch1��$b��\Для поиска определённого товара на складе введите��$
��Left=�$��34��$��8,-2,0,0q�$	��Top=�$	��372��$��14(���Z� �� -"   $ tbSearchProductCode��$
��Left=�$��34��$��	536,0,0,0q�$
 Wrap=�$	��Top=�$	��116��$��14(�.������  �� 
TargetType" �� �� Triggers P�   P� Property  �@=�HasError! P� Value! . . ��� " �� Property" �@�ToolTip# �� Value# �� $ �� RelativeSource$ �� *Self+$) #(Validation.Errors)[0].ErrorContent��� " �@=�ErrorTemplate# �� TextErrorTemplate��.R�]� .+��� 
���  ��$��1q�._��� .������ $
 Name�% �� ValidatesOnDataErrors% . & �� UpdateSourceTrigger$& PropertyChanged=�.��  -#   $ dgSearch��$��	5,40,5,45q� .     $+ %Наименование товара��$	 120  �� *commodity_name+  $ Код товара��$	 125  �� *coralclub_id+  $ Описание��$	 125  �� *desc+  $ Срок годности��$	 125  �� *expiration_date+  $" Стоимость (в уе)��$	 125  �� *price_value+  $* $Стоимость (в баллах)��$	 125  �� *point_value+  $ Количество��$	 125  �� *
quantity+�� -$   $ lSearch2��$��или��$
��Left=�$��32��$��	496,0,0,0q�$	��Top=�$��36��$��14(��� -%   $ bClose��$��
Выйти��$��Right=�$��34��$��	0,15,13,0q�$	��Top=�$��91��$��14(�
�������
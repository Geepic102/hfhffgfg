   M S B A M L   `   `   `  ����  WpfApp4    ProductApp.OrderWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$'  !Оформление заказа��5   	   $	��400��6#   $	��600��60   .��5      � .+�5   
   �� -   5        $��ProductComboBox��5      $
��Left=�6*   $��	10,10,0,0q�6E   $	��Top=�6X   $	��200��6p    �� DisplayMemberPath$
 Name��6|   �� 5   
   $
��Left=�6   $��	10,40,0,0q�60   $	��Top=�6C   $	��200��6[   .+�5	      �� $*��$Введите количество:��6   $��0,0,0,5q�63   �� 5
      -   5        $��QuantityTextBox��5
      5      �� 5   
   -   5        $$��Добавить в заказ��5      $
��Left=�6,   $	��Top=�6G   $	��200��6_   $��30��6k   $��
10,100,0,0q�6w   6�   �� 5   
   -   5        $��OrderListView��5      $
��Left=�6(   $	��200��6C   $��10,140,10,50q�6P   $	��Top=�6f   $	��560��6~    �� View 5      �� 5      .P�5      �� $��Продукт��6%    �� Width$	 250��6d    �� DisplayMemberBinding 66   �� *Product.Name+6d   �� 5      $��Количество��6%   $	 100��6c    69   �� *
Quantity+6c   �� 5      $��Цена��6%   $	 100��6b    63   �� *Product.Price+6b   5      5      �� 5   
   -   5        $!��Оформить заказ��5      $
��Left=�6*   $��Bottom=�6E   $	��200��6`   $��30��6l   $��	10,0,0,10q�6x   6�   5      5      
   M S B A M L   `   `   `  ����  	CineLingo$    CineLingo.Views.AddMovieWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$!  Добавить фильм��5   	   $	��650��5   	   $	��500��5   	    � WindowStartupLocation$ CenterScreen=�5   	   
������ .��5      �    	GridStyle#���   6   .+�5	   
   � $��15q�6    � RowDefinitions 5
      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5"      �� $��0��6   $$��Название фильма:��6!     
LabelStyle#���  6<   �� 5#      -   5         ��Name$ TitleTextBox��5#      $��1��6,   $��0,5,0,10q�69   �� 5&      $��2��6   $��Описание:��6!   #���  65   �� 5'      -   5        $ DescriptionTextBox��5'      $��3��62   $��0,5,0,10q�6?    �� AcceptsReturn .  �� TextWrapping$
 Wrap=�5(      $��60��63   �� 5+      $��4��6   $4��.Уровень английского (A1-C2):��6!   #���  6H   �� 5,      -   5         ��Name$ LevelComboBox��5,      $��5��6.   $��0,5,0,10q�6;   .@�5-      �� .��6   A16#   �� 5.      .��6   A26#   �� 5/      .��6   B16#   �� 50      .��6   B26#   �� 51      .��6   C16#   �� 52      .��6   C26#   53      �� 56      $��6��6   $)��#Ссылка на видео (URL):��6!   #���  6B   �� 57      -   5        $ VideoUrlTextBox��57      $��7��6/   $��0,5,0,10q�6<   �� 5:      $��8��6   $+��%Ссылка на постер (URL):��6!   #���  6C   �� 5;      -   5        $ PosterUrlTextBox��5;      $��9��60   $��0,5,0,10q�6=   �� 5>      $��10��6   $��Субтитры:��6"   $
��Bold"�66   $��0,15,0,5q�6H   #���  6Z   �� 5A      $��11��6   $"��Язык субтитров:��6"   #���  6<   �� 5B      -   5        $ SubtitleLanguageTextBox��5B      $��12��67   $��0,5,0,10q�6E   �� 5E      $��13��6   $-��'Файл субтитров (путь):��6"   #���  6C   ]� 5F      $��14��6   $��0,5,0,15q�6&   .+�5G      �� -   5         ��Name$ BrowseSubtitleButton��5G      $��Обзор...��5H      $��Right=�5I      $��5,0,0,0q�5J      $��80��5K        ButtonStyle#���  5L   0   �� 5M      -   5        $ SubtitleFileTextBox��5M      5N      �� 5Q      $��15��6    �� Orientation$ 
Horizontal=�6'   $��Right=�6@   .+�5R      �� -	   5        $ 	AddButton��5R      $��Добавить��6,   $	��100��6?   $��0,0,10,0q�6K   #���  6u   �� 5S      -
   5        $��Отмена��5S      $	��100��6*   #���  6Q   5T      5U      5V      5W      
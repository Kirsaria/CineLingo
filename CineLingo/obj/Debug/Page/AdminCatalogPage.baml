   M S B A M L   `   `   `  ����  	CineLingo%    CineLingo.Page.AdminCatalogPage   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5        N� Title$  AdminCatalogPage��5       � 	Resources 5
         BoolToVisibility �� 
TargetType  MovieItemStyle �� Property �� Value  MaterialDesignShadowBrush  TextBlockStyle  RoundedImageStyle �� Rect �� RadiusX �� RadiusY��%  &	        &	      &	 �    �� 5   
   6'   �� 5   
   " ��6   .�5      ��  � ��$	 115��6&   �� 5       � ��$	 220��6'   �� 5       � ��$ 10q�6'   �� 5       � ��
 ���� 6+   �� 5       � ��# �  6,   �� 5       �@��CornerRadius$ 20��6-   �� 5       �@�ToolTip 5      _� 5      .��5      �� $
��Wrap=�5   $   $	��300��5   $   #���  63   ��5   $   �� *Description_movie+5   3   5      5      5      �� 5   
   " ��6*   .�5   
   ��  �@��CornerRadius$ 8��6)   �� 5    
    �@J�Clip 5!      �� 5"      $ 0,0,115,170�6$   $ 8Q�67   $ 8Q�6C   5$      �� 5%   
    � ��$	 170��6#   �� 5&   
    � ��$ 0,0,0,5q�6#   5'      5(      .��5*      �� .��5+   
   �� .+�5-      �� $��0,0,0,3q�6   $��0,0,0,20q�6/    �� CornerRadius$ 30��6A     BorderCatalogStyle#���  6S   ._�5.      �� .+�5/      �� $��A1��6    $��18(�6*   $
��Bold"�68   $��	10,10,0,5q�6J   #���  6]   � 50      	 � Orientation$	 
Horizontal=�6    .+�51      �� -   5        
 ��Name$
 A1Movies��51   '   ��69   �� ��52      �� 53   "   .R�54   &   � $	 
Horizontal=�60   55   #   ��57      �� 58   "   .R�59   &   �� #��[ 6-   ._�5:   *   � -   5         � RowDefinitions 5;   .   �� 5<   2   $
��Auto �6@   �� 5=   2   $��* �6@   �� 5>   2   $
��Auto �6@   .+�5B   .   �� #��[ 65   ._�5C   2   �� $��UniformToFill=�6U   $	��170��6m   $	��120��6z   ��68   �� *	PosterUrl+6z   5D   /   �� 5F   .   $��1��69   $��10q�6F   .+�5G   2   �� $
��Bold"�6S   $
��Wrap=�6e   #���  6y   ��6<   �� *Title+6y   5H   /   5I   +   5J   '   5K   #   5M      �� 5N      -   5        $��+��5N   !   $��10q�6-   $��30��69   $��30��6D   $��Center=�6P   
 � Tag$ A1��5O   >   5P      5Q      5R      �� 5U      $��0,0,0,3q�6   $��0,0,0,20q�6/   $ 30��6A   #���  6S   ._�5V      �� .+�5W      �� $��A2��6    $��18(�6*   $
��Bold"�68   $��	10,10,0,5q�6J   #���  6]   � 5X      $	 
Horizontal=�6    .+�5Y      �� -   5        $
 A2Movies��5Y   '   ��69   �� ��5Z      �� 5[   "   .R�5\   &   � $	 
Horizontal=�60   5]   #   ��5_      �� 5`   "   .R�5a   &   �� #��[ 6-   ._�5b   *   � -   5         5c   .   �� 5d   2   $
��Auto �6@   �� 5e   2   $��* �6@   �� 5f   2   $
��Auto �6@   .+�5h   .   �� #��[ 65   ._�5i   2   �� $��UniformToFill=�6U   $	��170��6m   $	��120��6z   ��68   �� *	PosterUrl+6z   5j   /   �� 5k   .   $��1��69   $��10q�6F   .+�5l   2   �� $
��Bold"�6S   $
��Wrap=�6e   #���  6y   ��6<   �� *Title+6y   5m   /   5n   +   5o   '   5p   #   5r      �� 5s      -   5        $��+��5s   !   $��10q�6-   $��30��69   $��30��6D   $��Center=�6P   $ A2��5t   >   5u      5v      5w      �� 5z      $��0,0,0,3q�6   $��0,0,0,20q�6/   $ 30��6A   #���  6S   ._�5{      �� .+�5|      �� $��B1��6    $��18(�6*   $
��Bold"�68   $��	10,10,0,5q�6J   #���  6]   � 5}      $	 
Horizontal=�6    .+�5~      �� -   5        $
 B1Movies��5~   '   ��69   �� ��5      �� 5�   "   .R�5�   &   � $	 
Horizontal=�60   5�   #   ��5�      �� 5�   "   .R�5�   &   �� $	��200��6-   $	��300��69   $��10q�6F   
�������$��1q�6e   #��[ 6z   ._�5�   *   � -   5         5�   .   �� 5�   2   $
��Auto �6@   �� 5�   2   $��* �6@   �� 5�   2   $
��Auto �6@   .+�5�   .   �� #��[ 65   ._�5�   2   �� $��UniformToFill=�6U   $	��170��6m   $	��120��6z   ��68   �� *	PosterUrl+6z   5�   /   �� 5�   .   $��1��69   $��10q�6F   .+�5�   2   �� $
��Bold"�6S   $
��Wrap=�6e   #���  6y   ��6<   �� *Title+6y   5�   /   5�   +   5�   '   5�   #   5�      �� 5�      -	   5        $��+��5�   !   $��10q�6-   $��30��69   $��30��6D   $��Center=�6P   $ B1��5�   >   5�      5�      5�      �� 5�      $��0,0,0,3q�6   $��0,0,0,20q�6/   $ 30��6A   #���  6S   ._�5�      �� .+�5�      �� $��B2��6    $��18(�6*   $
��Bold"�68   $��	10,10,0,5q�6J   #���  6]   � 5�      $	 
Horizontal=�6    .+�5�      �� -
   5        $
 B2Movies��5�   '   ��69   �� ��5�      �� 5�   "   .R�5�   &   � $	 
Horizontal=�60   5�   #   ��5�      �� 5�   "   .R�5�   &   �� $	��200��6-   $	��300��69   $��10q�6F   
�������$��1q�6e   #��[ 6z   ._�5�   *   � -   5         5�   .   �� 5�   2   $
��Auto �6@   �� 5�   2   $��* �6@   �� 5�   2   $
��Auto �6@   .+�5�   .   �� #��[ 65   ._�5�   2   �� $��UniformToFill=�6U   $	��170��6m   $	��120��6z   ��68   �� *	PosterUrl+6z   5�   /   �� 5�   .   $��1��69   $��10q�6F   .+�5�   2   �� $
��Bold"�6S   $
��Wrap=�6e   #���  6y   ��6<   �� *Title+6y   5�   /   5�   +   5�   '   5�   #   5�      �� 5�      -   5        $��+��5�   !   $��10q�6-   $��30��69   $��30��6D   $��Center=�6P   $ B2��5�   >   5�      5�      5�      �� 5�      $��0,0,0,3q�6   $��0,0,0,20q�6/   $ 30��6A   #���  6S   ._�5�      �� .+�5�      �� $��C1��6    $��18(�6*   $
��Bold"�68   $��	10,10,0,5q�6J   #���  6]   � 5�      $	 
Horizontal=�6    .+�5�      �� -   5        $
 C1Movies��5�   '   ��69   �� ��5�      �� 5�   "   .R�5�   &   � $	 
Horizontal=�60   5�   #   ��5�      �� 5�   "   .R�5�   &   �� $	��200��6-   $	��300��69   $��10q�6F   
�������$��1q�6e   #��[ 6z   ._�5�   *   � -   5         5�   .   �� 5�   2   $
��Auto �6@   �� 5�   2   $��* �6@   �� 5�   2   $
��Auto �6@   .+�5�   .   �� #��[ 65   ._�5�   2   �� $��UniformToFill=�6U   $	��170��6m   $	��120��6z   ��68   �� *	PosterUrl+6z   5�   /   �� 5�   .   $��1��69   $��10q�6F   .+�5�   2   �� $
��Bold"�6S   $
��Wrap=�6e   #���  6y   ��6<   �� *Title+6y   5�   /   5�   +   5�   '   5�   #   5�      �� 5�      -   5        $��+��5�   !   $��10q�6-   $��30��69   $��30��6D   $��Center=�6P   $ C1��5�   >   5�      5�      5�      �� 5�      $��0,0,0,3q�6   $��0,0,0,20q�6/   $ 30��6A   #���  6S   ._�5�      �� .+�5�      �� $��C2��6    $��18(�6*   $
��Bold"�68   $��	10,10,0,5q�6J   #���  6]   � 5�      $	 
Horizontal=�6    .+�5�      �� -   5        $
 C2Movies��5�   '   ��69   �� ��5�      �� 5�   "   .R�5�   &   � $	 
Horizontal=�60   5�   #   ��5�      �� 5�   "   .R�5�   &   �� $	��200��6-   $	��300��69   $��10q�6F   
�������$��1q�6e   #��[ 6z   ._�5�   *   � -   5         5�   .   �� 5�   2   $
��Auto �6@   �� 5�   2   $��* �6@   �� 5�   2   $
��Auto �6@   .+�5�   .   �� #��[ 65   ._�5�   2   �� $��UniformToFill=�6U   $	��170��6m   $	��120��6z   ��68   �� *	PosterUrl+6z   5�   /   �� 5�   .   $��1��69   $��10q�6F   .+�5   2   �� $
��Bold"�6S   $
��Wrap=�6e   #���  6y   ��6<   �� *Title+6y   5  /   5  +   5  '   5  #   5     �� 5     -   5        $��+��5  !   $��10q�6-   $��30��69   $��30��6D   $��Center=�6P   $ C2��5  >   5	     5
     5     5     5     5     
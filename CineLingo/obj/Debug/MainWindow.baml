   M S B A M L   `   `   `  ����D  @CineLingo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:CineLingo	CineLingo  5   	       CineLingo.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   _ [MaterialDesignThemes.Wpf, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   #localclr-namespace:CineLingo  5   	     � Title$  	CineLingo��5	   	   $	��350��5
   	   $	��825��5   	    � WindowStyle$
 None=�5   	    � WindowState$ 	Maximized=�5   	    � AllowsTransparency .  � Icon$ icon.ico��5   	   '  !System.Windows.Shell.WindowChrome   WindowChrome 5        5   
      CaptionHeight$ 10Q�6   .��5      �    	GridStyle#���   6   .+�5   
   ��   BorderStyle#���  6   $	��Top=�67   $��60��6O   $��3��6[   $��0,0,0,0q�6o    J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         Color$ #380F17��6#   	   	Direction$		 200Q�63   
   
BlurRadius$
 10Q�6C      ShadowDepth$ 5Q�6S      Opacity$	 0.5Q�6c   ._�5      � $	��Top=�6   $��60��6+   .+�5      ��  �� Orientation$ 
Horizontal=�6   $��Center=�66   $
��Left=�6Q   $��20,0,0,0q�6l   .+�5      �� -   5          ButtonStyle#���  5      $��Каталог��6C   $
��10,0q�6o   $
��20 5q�6}   �� 5      -   5        #���  5      $��Словарь��6C   $
��10,0q�6r   $
��20 5q�6�   �� 5      -   5        #���  5      $��Видеоплеер��6C   $
��10,0q�6v   $
��20 5q�6�   5      �� 5      $ 
Horizontal=�6   $��Center=�66   $��Right=�6Q   $��1��6m   $
��10,0q�6}   .+�5      �� -   5         ��Name$ SwitchThemeButton��5        	DarkStyle#���  68   $
��10 0q�6p   �� 5       -   5        $
 user��5         	UserStyle#���  6+   $
��10,0q�6h   �� 5!      -   5        $ 
hidewindow��5!        MinimizeStyle#���  61   $
��10,0q�6v   �� 5"      -   5        $ 
fullscreen��5"        FullscreenStyle#���  61   $
��10,0q�6x   �� 5#      -   5        $
 exit��5#        
CloseStyle#���  6+   $
��10,0q�6f   5$      5%      5&       � 5'   
   -	   5          �Name$ 	MainFrame��5'        � NavigationUIVisibility$ Hidden=�6#   $��0,60,0,0q�6C   5(      5)      
Imports DianMing.BinarySerializer
Imports DianMing.SchoolClass
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Threading

Partial Public Class MainWindow
    Inherits Window

    Private ReadOnly _rand As New Random()
    Private isAnimating As Boolean = False

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Go_to_EditStudents(sender As Object, e As RoutedEventArgs)
        If isAnimating Then Return
        Dim editStudentsWindow As New EditStudents() With {.Owner = Me}
        AddHandler editStudentsWindow.Closed, Sub() Display_Buttons(Me, New RoutedEventArgs()) ' refresh buttons if classes changed
        editStudentsWindow.ShowDialog()
    End Sub

    Private Sub Go_to_EditClasses(sender As Object, e As RoutedEventArgs)
        If isAnimating Then Return
        Dim editClassesWindow As New EditClasses() With {.Owner = Me}
        AddHandler editClassesWindow.Closed, Sub() Display_Buttons(Me, New RoutedEventArgs())
        editClassesWindow.ShowDialog()
    End Sub

    Private Sub Exit_App(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub Display_Buttons(sender As Object, e As RoutedEventArgs)
        If ButtonsPanel Is Nothing Then Return
        ButtonsPanel.Children.Clear()

        Dim classes As List(Of SchoolClass) = LoadClassesSafely()

        For Each cls In classes
            Dim classButton As New Button With {
                .Content = cls.ClassName,
                .Width = 160,
                .Height = 60,
                .Margin = New Thickness(8),
                .FontSize = 28,
                .FontFamily = New FontFamily("Microsoft YaHei")
            }
            AddHandler classButton.Click, Sub(s, args) AnimateRandomSelection(cls.ClassName)
            ButtonsPanel.Children.Add(classButton)
        Next
    End Sub

    Private Function LoadClassesSafely() As List(Of SchoolClass)
        Try
            Return BinarySerializer.Deserialize(Of List(Of SchoolClass))()
        Catch ex As Exception
            If ex.Message.Contains("File does not exist") Then
                Return New List(Of SchoolClass)()
            Else
                MessageBox.Show("Error loading classes: " & ex.Message)
                Return New List(Of SchoolClass)()
            End If
        End Try
    End Function

    Private Sub AnimateRandomSelection(className As String)
        If isAnimating Then Return

        ' Reload latest data in case students changed in EditStudents
        Dim classes = LoadClassesSafely()
        Dim cls = classes.FirstOrDefault(Function(c) c.ClassName = className)
        If cls Is Nothing OrElse cls.Members Is Nothing OrElse cls.Members.Count = 0 Then
            If SelectedMemberLabel IsNot Nothing Then SelectedMemberLabel.Content = "(无学生)"
            Return
        End If

        isAnimating = True
        SetButtonsEnabled(False)

        Dim members = cls.Members.ToList()
        Dim iteration As Integer = 0
        Dim finalName As String = Nothing

        Dim timer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(50)} ' changed from 100ms to 50ms
        AddHandler timer.Tick,
            Sub()
                iteration += 1
                members = members.OrderBy(Function(x) _rand.Next()).ToList()
                finalName = members(0)
                SelectedMemberLabel.Content = finalName
                If iteration >= 50 Then
                    timer.Stop()
                    HighlightFinalSelection(finalName)
                End If
            End Sub
        timer.Start()
    End Sub

    Private Sub HighlightFinalSelection(finalName As String)
        If SelectedMemberLabel Is Nothing Then Return
        Dim originalBrush = SelectedMemberLabel.Foreground
        SelectedMemberLabel.Foreground = Brushes.Red

        Dim revertTimer As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(1.5)}
        AddHandler revertTimer.Tick,
            Sub()
                revertTimer.Stop()
                SelectedMemberLabel.Foreground = originalBrush
                SetButtonsEnabled(True)
                isAnimating = False
            End Sub
        revertTimer.Start()
    End Sub

    Private Sub SetButtonsEnabled(enabled As Boolean)
        If ButtonsPanel Is Nothing Then Return
        For Each child In ButtonsPanel.Children
            Dim btn = TryCast(child, Button)
            If btn IsNot Nothing Then btn.IsEnabled = enabled
        Next
    End Sub
End Class

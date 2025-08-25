Imports DianMing.BinarySerializer
Imports DianMing.SchoolClass
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports Microsoft.Win32
Imports System.IO
Imports System.Text.RegularExpressions

Partial Public Class EditStudents
    Inherits Window

    Private classesData As New List(Of SchoolClass)()
    Private Const TextBoxDesiredWidth As Double = 300

    Private Sub EditStudents_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadClasses()
    End Sub

    Private Sub LoadClasses()
        Try
            classesData = BinarySerializer.Deserialize(Of List(Of SchoolClass))()
        Catch ex As Exception
            classesData = New List(Of SchoolClass)()
            If Not ex.Message.Contains("File does not exist") Then
                MessageBox.Show("加载班级失败: " & ex.Message)
            End If
        End Try
        CurrentClassSelection.ItemsSource = classesData.Select(Function(c) c.ClassName).ToList()
        If CurrentClassSelection.Items.Count > 0 Then
            CurrentClassSelection.SelectedIndex = 0
        Else
            StudentList.Items.Clear()
            NoMembersNotice.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub RefreshMembers()
        Dim selectedName = TryCast(CurrentClassSelection.SelectedItem, String)
        Dim cls = classesData.FirstOrDefault(Function(c) c.ClassName = selectedName)
        StudentList.Items.Clear()
        If cls Is Nothing OrElse cls.Members Is Nothing OrElse cls.Members.Count = 0 Then
            NoMembersNotice.Visibility = Visibility.Visible
            Return
        End If
        NoMembersNotice.Visibility = Visibility.Collapsed
        For Each m In cls.Members
            StudentList.Items.Add(CreateStudentEntry(m, False))
        Next
    End Sub

    Private Sub CurrentClassSelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        RefreshMembers()
    End Sub

    Private Sub AddStudent_Click(sender As Object, e As RoutedEventArgs)
        If CurrentClassSelection.SelectedItem Is Nothing Then
            MessageBox.Show("请先选择一个班级")
            Return
        End If
        NoMembersNotice.Visibility = Visibility.Collapsed
        StudentList.Items.Add(CreateStudentEntry("", True))
    End Sub

    Private Function CreateStudentEntry(name As String, editing As Boolean) As Border
        Dim border As New Border With {.BorderBrush = Brushes.LightGray, .BorderThickness = New Thickness(1), .CornerRadius = New CornerRadius(4), .Padding = New Thickness(6), .Margin = New Thickness(0, 0, 0, 4)}

        Dim grid As New Grid With {.HorizontalAlignment = HorizontalAlignment.Stretch}
        grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

        Dim txt As New TextBox With {.Text = name, .FontSize = 14, .VerticalContentAlignment = VerticalAlignment.Center, .Visibility = If(editing, Visibility.Visible, Visibility.Collapsed)}
        txt.MinWidth = TextBoxDesiredWidth
        Dim lbl As New TextBlock With {.Text = name, .FontSize = 14, .VerticalAlignment = VerticalAlignment.Center, .Visibility = If(editing, Visibility.Collapsed, Visibility.Visible), .TextTrimming = TextTrimming.CharacterEllipsis}

        Dim buttons As New StackPanel With {.Orientation = Orientation.Horizontal, .HorizontalAlignment = HorizontalAlignment.Right}
        Grid.SetColumn(buttons, 1)

        Dim makeBtn = Function(caption As String) New Button With {.Content = caption, .Padding = New Thickness(6, 0, 6, 0), .Margin = New Thickness(4, 0, 0, 0), .MinWidth = 50, .Height = 26}
        Dim btnOk = makeBtn("确认")
        Dim btnDel = makeBtn("删除")
        Dim btnEdit = makeBtn("编辑")

        Dim toEdit = Sub()
                         txt.Text = lbl.Text
                         txt.Visibility = Visibility.Visible
                         lbl.Visibility = Visibility.Collapsed
                         btnOk.Visibility = Visibility.Visible
                         btnDel.Visibility = Visibility.Visible
                         btnEdit.Visibility = Visibility.Collapsed
                     End Sub
        Dim toRead = Sub()
                         lbl.Text = txt.Text
                         txt.Visibility = Visibility.Collapsed
                         lbl.Visibility = Visibility.Visible
                         btnOk.Visibility = Visibility.Collapsed
                         btnDel.Visibility = Visibility.Visible
                         btnEdit.Visibility = Visibility.Visible
                     End Sub

        AddHandler btnOk.Click,
            Sub()
                Dim newName = txt.Text.Trim()
                If newName.Length = 0 Then
                    MessageBox.Show("学生姓名不能为空")
                    Return
                End If
                Dim cls = classesData.FirstOrDefault(Function(c) c.ClassName = CStr(CurrentClassSelection.SelectedItem))
                If cls Is Nothing Then Return
                Dim old = CStr(border.Tag)
                If old Is Nothing Then
                    If cls.Members Is Nothing Then cls.Members = New List(Of String)()
                    If cls.Members.Contains(newName) Then
                        MessageBox.Show("该学生已存在") : Return
                    End If
                    cls.Members.Add(newName)
                Else
                    If newName <> old AndAlso cls.Members.Contains(newName) Then
                        MessageBox.Show("该学生已存在") : Return
                    End If
                    Dim i = cls.Members.IndexOf(old)
                    If i >= 0 Then cls.Members(i) = newName
                End If
                border.Tag = newName
                BinarySerializer.Serialize(classesData, Nothing)
                toRead()
            End Sub

        AddHandler btnDel.Click,
            Sub()
                If MessageBox.Show("确定要删除该学生?", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                    Dim cls = classesData.FirstOrDefault(Function(c) c.ClassName = CStr(CurrentClassSelection.SelectedItem))
                    If cls IsNot Nothing Then
                        Dim old = CStr(border.Tag)
                        If old IsNot Nothing AndAlso cls.Members.Contains(old) Then
                            cls.Members.Remove(old)
                            BinarySerializer.Serialize(classesData, Nothing)
                        End If
                    End If
                    StudentList.Items.Remove(border)
                    If StudentList.Items.Count = 0 Then NoMembersNotice.Visibility = Visibility.Visible
                End If
            End Sub

        AddHandler btnEdit.Click, Sub() toEdit()

        If editing Then
            btnOk.Visibility = Visibility.Visible
            btnDel.Visibility = Visibility.Visible
            btnEdit.Visibility = Visibility.Collapsed
        Else
            btnOk.Visibility = Visibility.Collapsed
            btnDel.Visibility = Visibility.Visible
            btnEdit.Visibility = Visibility.Visible
            border.Tag = name
        End If

        buttons.Children.Add(btnOk)
        buttons.Children.Add(btnEdit)
        buttons.Children.Add(btnDel)

        grid.Children.Add(txt)
        grid.Children.Add(lbl)
        grid.Children.Add(buttons)
        border.Child = grid
        Return border
    End Function

    Private Sub ImportStudents_Click(sender As Object, e As RoutedEventArgs)
        Dim cls = classesData.FirstOrDefault(Function(c) c.ClassName = CStr(CurrentClassSelection.SelectedItem))
        If cls Is Nothing Then MessageBox.Show("请先选择一个班级") : Return

        Dim dlg As New OpenFileDialog With {.Title = "选择学生名单", .Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*", .Multiselect = False}
        If dlg.ShowDialog(Me) <> True Then Return
        Dim path = dlg.FileName
        If Not File.Exists(path) Then MessageBox.Show("文件不存在") : Return

        Dim content As String
        Try
            content = File.ReadAllText(path)
        Catch ex As Exception
            MessageBox.Show("读取文件失败: " & ex.Message) : Return
        End Try

        cls.Members = New List(Of String)()
        Dim raw = Regex.Split(content, "[\s,，;；、]+")
        For Each r In raw
            Dim nm = r.Trim()
            If nm.Length > 0 AndAlso Not cls.Members.Contains(nm) Then cls.Members.Add(nm)
        Next
        BinarySerializer.Serialize(classesData, Nothing)
        RefreshMembers()
    End Sub
End Class

Imports DianMing.BinarySerializer
Imports DianMing.SchoolClass
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Linq

Partial Public Class EditClasses
    Inherits Window

    Private classesData As List(Of SchoolClass) = New List(Of SchoolClass)()

    Private Sub EditClasses_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadClasses()
    End Sub

    Private Sub LoadClasses()
        ClassesList.Children.Clear()
        Try
            classesData = BinarySerializer.Deserialize(Of List(Of SchoolClass))()
        Catch ex As Exception
            If ex.Message.Contains("File does not exist") Then
                classesData = New List(Of SchoolClass)()
            Else
                MessageBox.Show("加载班级失败: " & ex.Message)
                classesData = New List(Of SchoolClass)()
            End If
        End Try
        For Each cls In classesData
            ClassesList.Children.Add(CreateClassEntry(cls.ClassName, isEditing:=False))
        Next
    End Sub

    Private Sub SaveClasses()
        Try
            BinarySerializer.Serialize(classesData, Nothing)
        Catch ex As Exception
            MessageBox.Show("保存失败: " & ex.Message)
        End Try
    End Sub

    Private Sub AddClass_Click(sender As Object, e As RoutedEventArgs)
        ClassesList.Children.Add(CreateClassEntry("", True))
    End Sub

    Private Function IsNullOrWhiteSpaceCustom(value As String) As Boolean
        If value Is Nothing Then Return True
        If value.Trim().Length = 0 Then Return True
        Return False
    End Function

    Private Function CreateClassEntry(name As String, isEditing As Boolean) As Border
        Dim border As New Border With {
            .BorderBrush = Brushes.LightGray,
            .BorderThickness = New Thickness(1),
            .CornerRadius = New CornerRadius(4),
            .Padding = New Thickness(8),
            .Margin = New Thickness(0, 0, 8, 6),
            .Width = 420,
            .HorizontalAlignment = HorizontalAlignment.Left
        }

        Dim grid As New Grid()
        grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

        Dim textBox As New TextBox With {
            .Text = name,
            .FontSize = 16,
            .VerticalContentAlignment = VerticalAlignment.Center,
            .Visibility = If(isEditing, Visibility.Visible, Visibility.Collapsed)
        }
        Dim label As New TextBlock With {
            .Text = name,
            .FontSize = 16,
            .VerticalAlignment = VerticalAlignment.Center,
            .Visibility = If(isEditing, Visibility.Collapsed, Visibility.Visible)
        }

        Dim buttonsPanel As New StackPanel With {
            .Orientation = Orientation.Horizontal,
            .HorizontalAlignment = HorizontalAlignment.Right
        }
        Grid.SetColumn(buttonsPanel, 1)

        ' Helper to create icon button
        Dim createBtn = Function(glyph As Integer, tooltip As String) As Button
                            Return New Button With {
                                 .Content = New TextBlock With {.FontFamily = New FontFamily("Segoe MDL2 Assets"), .Text = Char.ConvertFromUtf32(glyph), .FontSize = 16, .HorizontalAlignment = HorizontalAlignment.Center, .VerticalAlignment = VerticalAlignment.Center},
                                 .ToolTip = tooltip,
                                 .Width = 34,
                                 .Height = 34,
                                 .Margin = New Thickness(4, 0, 0, 0)
                             }
                        End Function

        Dim confirmBtn As Button = createBtn(&HE10B, "确认")   ' Accept
        Dim deleteBtn As Button = createBtn(&HE74D, "删除")    ' Delete
        Dim editBtn As Button = createBtn(&HE70F, "编辑")      ' Edit

        Dim showEditState = Sub()
                                textBox.Text = label.Text
                                textBox.Visibility = Visibility.Visible
                                label.Visibility = Visibility.Collapsed
                                confirmBtn.Visibility = Visibility.Visible
                                deleteBtn.Visibility = Visibility.Visible
                                editBtn.Visibility = Visibility.Collapsed
                            End Sub

        Dim showReadOnlyState = Sub()
                                    label.Text = textBox.Text
                                    textBox.Visibility = Visibility.Collapsed
                                    label.Visibility = Visibility.Visible
                                    confirmBtn.Visibility = Visibility.Collapsed
                                    deleteBtn.Visibility = Visibility.Visible
                                    editBtn.Visibility = Visibility.Visible
                                End Sub

        ' Confirm handler
        AddHandler confirmBtn.Click,
            Sub()
                Dim newName = textBox.Text.Trim()
                If IsNullOrWhiteSpaceCustom(newName) Then
                    MessageBox.Show("班级名称不能为空")
                    Return
                End If
                ' Duplicate check (ignore same entry if renaming to itself)
                Dim originalName = CStr(border.Tag)
                If classesData.Any(Function(c) c.ClassName = newName AndAlso (originalName Is Nothing OrElse c.ClassName <> originalName)) Then
                    MessageBox.Show("班级名称已存在")
                    Return
                End If

                If originalName Is Nothing Then
                    ' New class
                    Dim cls As New SchoolClass With {.ClassName = newName}
                    classesData.Add(cls)
                Else
                    ' Rename existing
                    Dim existing = classesData.FirstOrDefault(Function(c) c.ClassName = originalName)
                    If existing IsNot Nothing Then existing.ClassName = newName
                End If
                border.Tag = newName
                SaveClasses()
                showReadOnlyState()
            End Sub

        ' Delete handler
        AddHandler deleteBtn.Click,
            Sub()
                Dim result = MessageBox.Show("确定要删除该班级?", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                If result = MessageBoxResult.Yes Then
                    Dim existingName = CStr(border.Tag)
                    If existingName IsNot Nothing Then
                        Dim existing = classesData.FirstOrDefault(Function(c) c.ClassName = existingName)
                        If existing IsNot Nothing Then classesData.Remove(existing)
                        SaveClasses()
                    End If
                    ClassesList.Children.Remove(border)
                End If
            End Sub

        ' Edit handler
        AddHandler editBtn.Click, Sub() showEditState()

        ' Initial button visibility
        If isEditing Then
            confirmBtn.Visibility = Visibility.Visible
            deleteBtn.Visibility = Visibility.Visible
            editBtn.Visibility = Visibility.Collapsed
        Else
            confirmBtn.Visibility = Visibility.Collapsed
            deleteBtn.Visibility = Visibility.Visible
            editBtn.Visibility = Visibility.Visible
            border.Tag = name ' store original name
        End If

        buttonsPanel.Children.Add(confirmBtn)
        buttonsPanel.Children.Add(editBtn)
        buttonsPanel.Children.Add(deleteBtn)

        grid.Children.Add(textBox)
        grid.Children.Add(label)
        grid.Children.Add(buttonsPanel)

        border.Child = grid
        Return border
    End Function
End Class

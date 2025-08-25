Partial Class Application
    Inherits System.Windows.Application

    ' Shared startup path for the application (folder where the executable is running)
    Friend Shared ReadOnly StartupPath As String = AppDomain.CurrentDomain.BaseDirectory

End Class

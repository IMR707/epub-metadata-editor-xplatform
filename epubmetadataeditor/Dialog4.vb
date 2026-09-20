Imports System.Windows.Forms

Public Class Dialog4
    Dim button As String
    Public Property ButtonClicked() As String
        Get
            Return button
        End Get
        Set(ByVal value As String)
            button = value
        End Set
    End Property
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ButtonClicked = "YesToAll"
        Me.Hide()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        ButtonClicked = "Yes"
        Me.Hide()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        ButtonClicked = "No"
        Me.Hide()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        ButtonClicked = "NoToAll"
        Me.Hide()
    End Sub

    Private Sub Dialog4_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        ' Make the "No" button default every time
        Button3.Select()
    End Sub
End Class

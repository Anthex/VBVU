Imports CSCore
Imports CSCore.CoreAudioAPI
Imports System.IO.Ports


Public Class Form1

    Dim port As New SerialPort

    Shared CurrentProcessID As Integer = 8176


    Private Shared Function GetDefaultAudioSessionManager2(dataFlow As DataFlow) As AudioSessionManager2
        Using enumerator = New MMDeviceEnumerator()
            Using device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia)
                Dim sessionManager = AudioSessionManager2.FromMMDevice(device)
                Return sessionManager
            End Using
        End Using
    End Function

    Dim sessionManager As AudioSessionManager2
    Dim sessionEnumerator As AudioSessionEnumerator

    Dim leftval, lastleft As Double
    Dim rightval, lastright As Double

    Dim processes As ArrayList
    Dim proccount As Int32
    Dim proclist As New ArrayList

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lastleft = lastright = 0
        port.PortName = "COM3"
        port.BaudRate = 115200
        port.StopBits = IO.Ports.StopBits.One
        port.DataBits = 8
        port.Parity = IO.Ports.Parity.None

        Try
            port.Open()
            Label3.Text = "Connected"
            Label3.ForeColor = Color.SteelBlue
        Catch ex As Exception
            Label3.Text = "Disconnected"
        End Try
        For Each p As Process In Process.GetProcesses
            proclist.Add(p.Id)
        Next

        sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render)
        sessionEnumerator = sessionManager.GetSessionEnumerator()
        ComboBox1.Items.Clear()
        For Each k As Double In proclist
            leftval = 0
            Try


                For Each session In sessionEnumerator
                    Using session2 = session.QueryInterface(Of AudioSessionControl2)()
                        If session2.ProcessID = k Then
                            Using AudioMeterInformation = session.QueryInterface(Of AudioMeterInformation)()
                                If AudioMeterInformation.GetMeteringChannelCount > 0 Then

                                    Try
                                        leftval = Convert.ToByte(AudioMeterInformation.GetChannelsPeakValues()(0) * 60)
                                    Catch ex As IndexOutOfRangeException
                                    End Try
                                End If
                            End Using
                        End If

                    End Using
                Next
                If leftval > 0 Then
                    ComboBox1.Items.Add(k)
                End If
            Catch ex As Exception

            End Try
        Next
        If ComboBox1.Items.Count > 0 Then

            Try
                CurrentProcessID = ComboBox1.Items(0)
                ComboBox1.SelectedIndex = 0
            Catch ex As ArgumentOutOfRangeException

            End Try
        End If
        ComboBox2.SelectedIndex = 2
        Me.Focus()
    End Sub



    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If port.IsOpen = False Then
            Try
                port.Open()
            Catch ex As Exception

            End Try

        Else

            Try


                For Each session In sessionEnumerator
                    Using session2 = session.QueryInterface(Of AudioSessionControl2)()
                        If session2.ProcessID = CurrentProcessID Then
                            Using AudioMeterInformation = session.QueryInterface(Of AudioMeterInformation)()
                                If AudioMeterInformation.GetMeteringChannelCount > 0 Then

                                    Try
                                        leftval = AudioMeterInformation.GetChannelsPeakValues()(0) * 60
                                        rightval = AudioMeterInformation.GetChannelsPeakValues()(1) * 60
                                        If (Math.Abs(leftval - lastleft) < 60) Then
                                            leftval = (leftval + lastleft * TrackBar1.Value) / (TrackBar1.Value + 1)
                                        End If
                                        If (Math.Abs(rightval - lastright) < 60) Then
                                            rightval = (rightval + lastright * TrackBar1.Value) / (TrackBar1.Value + 1)
                                        End If
                                        lastleft = leftval
                                        lastright = rightval
                                        Panel5.Width = rightval * 4
                                        Panel2.Width = leftval * 4

                                        Panel2.BackColor = Color.FromArgb(leftval * 3 + 50, 30, 30)
                                        Panel5.BackColor = Color.FromArgb(rightval * 3 + 50, 30, 30)
                                        ''Panel2.Location = New Point(0, 33 - leftval * 4 + 240)
                                        ''Panel5.Location = New Point(321, 33 - rightval * 4 + 240)
                                    Catch ex As IndexOutOfRangeException
                                    End Try
                                End If
                                port.Write(Chr(leftval))
                                port.Write(Chr(rightval + 64))
                            End Using
                        End If

                    End Using
                Next
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        CurrentProcessID = Convert.ToDouble(ComboBox1.SelectedItem.ToString)
        Button6.Text = "Lightbar Control - VU | " + Process.GetProcessById(CurrentProcessID).ProcessName
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        proclist.Clear()
        For Each p As Process In Process.GetProcesses
            proclist.Add(p.Id)
        Next
        ComboBox1.Items.Clear()
        For Each k As Double In proclist
            leftval = 0
            Try


                For Each session In sessionEnumerator
                    Using session2 = session.QueryInterface(Of AudioSessionControl2)()
                        If session2.ProcessID = k Then
                            Using AudioMeterInformation = session.QueryInterface(Of AudioMeterInformation)()
                                If AudioMeterInformation.GetMeteringChannelCount > 0 Then

                                    Try
                                        leftval = Convert.ToByte(AudioMeterInformation.GetChannelsPeakValues()(0) * 60)
                                    Catch ex As IndexOutOfRangeException
                                    End Try
                                End If
                            End Using
                        End If

                    End Using
                Next
                If leftval > 0 Then
                    ComboBox1.Items.Add(k)
                End If
            Catch ex As Exception

            End Try
        Next
        If ComboBox1.Items.Count > 0 Then

            Try
                CurrentProcessID = ComboBox1.Items(0)
                ComboBox1.SelectedIndex = 0
            Catch ex As ArgumentOutOfRangeException

            End Try
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If TextBox1.TextLength > 3 Then
            CurrentProcessID = CInt(TextBox1.Text)
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        TextBox1.Enabled = True
        Button2.Visible = False
        TextBox1.Width = 178
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Me.Close()

    End Sub


    Private Sub Button6_MouseDown(sender As Object, e As MouseEventArgs) Handles Button6.MouseDown
        Timer2.Start()
    End Sub

    Private Sub Button6_MouseUp(sender As Object, e As MouseEventArgs) Handles Button6.MouseUp
        Timer2.Stop()
        Me.Focus()

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ColorDialog1.ShowDialog()
        setPrimaryColor(ColorDialog1.Color.R, ColorDialog1.Color.G, ColorDialog1.Color.B)
        Button4.BackColor = ColorDialog1.Color
    End Sub

    Private Sub setPrimaryColor(r As Byte, g As Byte, b As Byte)
        Dim msg As String = Chr(Convert.ToByte(127)) + Chr(Convert.ToByte(r)) + Chr(Convert.ToByte(g)) + Chr(Convert.ToByte(b))
        If port.IsOpen Then

            Try
                port.Write(msg)
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub setSecondaryColor(r As Byte, g As Byte, b As Byte)
        Dim msg As String = Chr(Convert.ToByte(125)) + Chr(Convert.ToByte(r)) + Chr(Convert.ToByte(g)) + Chr(Convert.ToByte(b))
        If port.IsOpen Then

            Try
                port.Write(msg)
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub changeMode(mode As Integer)
        If port.IsOpen Then

            Try
                Timer1.Stop()
                port.Write(Chr(126))
                port.Write(Chr(mode))
                Timer1.Start()
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        changeMode(0)
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        changeMode(1)
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        changeMode(2)
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        changeMode(3)
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        changeMode(4)
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        changeMode(5)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ColorDialog2.ShowDialog()
        setSecondaryColor(ColorDialog2.Color.R, ColorDialog2.Color.G, ColorDialog2.Color.B)
        Button3.BackColor = ColorDialog2.Color
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Me.Location = New Point(MousePosition.X - Button6.Location.X - Button6.Width / 2, MousePosition.Y - Button6.Location.Y - Button6.Height / 2)
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        Timer1.Interval = CInt(1000 / CInt(ComboBox2.SelectedItem))
    End Sub

End Class




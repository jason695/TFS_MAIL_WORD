Imports System.IO
Imports System.Collections

Public Class Form1
    Dim Path As String = Application.StartupPath.ToString
    Dim dt_PRJ As DataTable

    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        ini()
        Button1_Click(sender, e)
    End Sub

    ''' <summary>
    ''' 下拉選單初始化
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ini()
        Dim itm As String = ""
        Dim i As Boolean = False

        'cmbPROJ 專案
        cmbPROJ.Items.Clear()
        cmbPROJ.Text = ""

        Dim sr As StreamReader = New StreamReader(Path + "\project.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = sr.ReadLine().Split(",")(0)

            If i = False Then
                i = True
            Else
                cmbPROJ.Items.Add(itm)
            End If
        End While
        sr.Close()

        dt_PRJ = TxtConvertToDataTable(Path + "\project.txt", "tmp", ",")

        'ComboBox1 換版單清單
        ComboBox1.Items.Clear()
        ComboBox1.Text = ""

        Dim str As String() = Directory.GetFileSystemEntries(Path, "*_SET.txt")
        Array.Reverse(str)

        For Each item As String In str
            ComboBox1.Items.Add(item.Replace(Path + "\", "").Replace("_SET.txt", ""))
        Next

        '其他
        cmbSASD.Items.Clear()
        cmbOK.Items.Clear()
        cmbUSER.Items.Clear()

        cmbSASD.Text = ""
        cmbOK.Text = ""
        cmbUSER.Text = ""

    End Sub

    ''' <summary>
    ''' 產TXT
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs)
        If txt換版單.Text = "" Then
            MessageBox.Show("換版單不能為空")
            Exit Sub
        End If

        Dim dialog As SaveFileDialog = New SaveFileDialog()
        dialog.CreatePrompt = False
        dialog.OverwritePrompt = True
        dialog.FileName = txt換版單.Text + ".txt"
        dialog.Filter = "All Files (*.*)|*.*"
        dialog.Title = "Save File"

        Dim savepos As String = ""
        If dialog.ShowDialog() = DialogResult.OK Then
            savepos = dialog.FileName

            '產生檔案
            Dim fileStream As FileStream = New FileStream(savepos, FileMode.Create)
            fileStream.Close()

            Dim sw As StreamWriter = New StreamWriter(savepos)

            Try
                sw.WriteLine(RichTextBox1.Text)
            Finally
                sw.Close()
            End Try
        End If
    End Sub

    Private Sub MAIL(m_subject As String, m_reciver As String, m_body As String)
        'REF: https://support.microsoft.com/zh-tw/help/287573/how-to-use-command-line-switches-to-create-a-pre-addressed-e-mail-mess
        '這裏我們查找系統的缺省郵件客戶程序，其他的客戶程序我沒有試驗過，不知道這種方式是否可行 
        Dim rKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("mailto\shell\open\command")

        If rKey IsNot Nothing Then
            '這裏查找Outlook應用程序所在位置，也可以用其他方式去查 
            Dim path As String = rKey.GetValue("").ToString() + " "
            path = path.Substring(0, path.IndexOf(" "))
            path = path.Replace("""", "")
            rKey.Close()

            Try
                '調用執行Outlook，主要注意後面的參數，附件的文件地址空格隔開 
                System.Diagnostics.Process.Start(path, "-c IPM.Note /m " + m_reciver + "&subject=" + m_subject + "&body=" + m_body + "")
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
            End Try
        End If
    End Sub

    Private Sub CmbPROJ_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbPROJ.SelectedIndexChanged
        Dim sel_itm As String = cmbPROJ.SelectedItem.ToString
        txt需求單.Text = ""
        txt換版單.Text = ""

        cmbSASD.Items.Clear()
        cmbOK.Items.Clear()
        cmbUSER.Items.Clear()

        dt換版日.Text = ""
        txtTSF_SN.Text = ""

        Dim itm() As String
        Dim sr As StreamReader = New StreamReader(Path + "\group.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = sr.ReadLine().Split(",")

            If itm(0) = sel_itm Then
                If itm(1) = "SA/SD" Then
                    cmbSASD.Items.Add(itm(2))
                End If

                If itm(1) = "核准" Then
                    cmbOK.Items.Add(itm(2))
                End If

                If itm(1) = "驗收" Then
                    cmbUSER.Items.Add(itm(2))
                End If
            End If
        End While
        sr.Close()

        Dim dr As DataRow
        dr = dt_PRJ.Rows.Find(sel_itm)
        txtPRJ.Text = dr(1).ToString + vbCrLf + dr(2).ToString
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        txt換版單.Text = ComboBox1.SelectedItem.ToString
    End Sub

    ''' <summary>
    ''' 寫設定
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        If txt換版單.Text = "" Then
            MessageBox.Show("換版單不能為空")
            Exit Sub
        End If

        Dim dialog As SaveFileDialog = New SaveFileDialog()
        dialog.CreatePrompt = False
        dialog.OverwritePrompt = True
        dialog.FileName = txt換版單.Text + "_SET.txt"
        dialog.Filter = "All Files (*.*)|*.*"
        dialog.Title = "Save File"

        Dim savepos As String = ""
        If dialog.ShowDialog() = DialogResult.OK Then

            savepos = dialog.FileName

            '產生檔案
            Dim fileStream As FileStream = New FileStream(savepos, FileMode.Create)
            fileStream.Close()

            Dim sw As StreamWriter = New StreamWriter(savepos)
            Try
                'sw.WriteLine(RichTextBox1.Text)
                sw.WriteLine(cmbPROJ.SelectedItem.ToString) '1專案
                sw.WriteLine(txt需求單.Text) '2需求單
                sw.WriteLine(txt換版單.Text) '3換版單
                sw.WriteLine(If(cmbSASD.SelectedItem Is Nothing, "", cmbSASD.SelectedText.ToString())) '4AP2
                sw.WriteLine(If(cmbOK.SelectedItem Is Nothing, "", cmbOK.SelectedItem.ToString())) '5核准
                sw.WriteLine(If(cmbUSER.SelectedItem Is Nothing, "", cmbUSER.SelectedItem.ToString())) '6USER
                sw.WriteLine(dt換版日.Text) '7換版日
                sw.WriteLine(txtTSF_SN.Text) '8TFS_SN
            Finally
                sw.Close()
            End Try
        End If
    End Sub

    ''' <summary>
    ''' 讀設定
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click

        If File.Exists(Path + "\\" + txt換版單.Text + "_set.txt") = False Then
            MessageBox.Show("換版單的設定檔不存在")
            Exit Sub
        End If

        Dim itm As String = ""
        Dim i As Integer = 1

        Dim sr As StreamReader = New StreamReader(Path + "\\" + txt換版單.Text + "_set.txt", System.Text.Encoding.UTF8)
        While sr.Peek() > -1
            itm = sr.ReadLine()
            If i = 1 Then
                cmbPROJ.SelectedItem = itm
                CmbPROJ_SelectedIndexChanged(sender, e)
            ElseIf i = 2 Then
                txt需求單.Text = itm
            ElseIf i = 3 Then
                txt換版單.Text = itm
            ElseIf i = 4 Then
                cmbSASD.SelectedItem = itm
            ElseIf i = 5 Then
                cmbOK.SelectedItem = itm
            ElseIf i = 6 Then
                cmbUSER.SelectedItem = itm
            ElseIf i = 7 Then
                dt換版日.Text = itm
            ElseIf i = 8 Then
                txtTSF_SN.Text = itm
            End If

            i += 1
        End While
        sr.Close()
    End Sub

    ''' <summary>
    ''' 產MAIL
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    ''' 
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim m_subject As String = replaceMail(RichTextBox1.Text)
        Dim m_reciver As String = ""
        Dim m_body As String = ""

        '[SIT]
        m_reciver = cmbSASD.SelectedItem.ToString
        m_body = replaceMail(RichTextBox2.Text)
        MAIL(m_subject, m_reciver, m_body)

        '[UAT]
        m_reciver = cmbUSER.SelectedItem.ToString
        m_body = replaceMail(RichTextBox3.Text)
        MAIL(m_subject, m_reciver, m_body)

        '[TFS]
        m_reciver = cmbOK.SelectedItem.ToString
        m_body = replaceMail(RichTextBox4.Text)
        MAIL(m_subject, m_reciver, m_body)

        '[TFS]
        m_reciver = txtSE.Text.ToString
        m_body = replaceMail(RichTextBox5.Text)
        MAIL(m_subject, m_reciver, m_body)
    End Sub

    ''' <summary>
    ''' 讀樣版
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim itm As String = ""
        Dim sr As StreamReader

        '--R1
        itm = ""
        sr = New StreamReader(Path + "\R1.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = itm + sr.ReadLine().ToString + vbLf
        End While

        RichTextBox1.Text = itm
        sr.Close()

        '--R2
        itm = ""
        sr = New StreamReader(Path + "\R2.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = itm + sr.ReadLine().ToString + vbLf
        End While

        RichTextBox2.Text = itm
        sr.Close()

        '--R3
        itm = ""
        sr = New StreamReader(Path + "\R3.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = itm + sr.ReadLine().ToString + vbLf
        End While

        RichTextBox3.Text = itm
        sr.Close()

        '--R4
        itm = ""
        sr = New StreamReader(Path + "\R4.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = itm + sr.ReadLine().ToString + vbLf
        End While

        RichTextBox4.Text = itm
        sr.Close()

        '--R5
        itm = ""
        sr = New StreamReader(Path + "\R5.txt", System.Text.Encoding.Default)
        While sr.Peek() > -1
            itm = itm + sr.ReadLine().ToString + vbLf
        End While

        RichTextBox5.Text = itm
        sr.Close()

        'MessageBox.Show("讀取完成!!")
    End Sub

    ''' <summary>
    ''' 寫樣版
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        '產生檔案
        '--R1
        Dim fileStream As FileStream = New FileStream(Path + "\R1.txt", FileMode.OpenOrCreate)
        fileStream.Close()
        Dim sw As StreamWriter = New StreamWriter(Path + "\R1.txt", False, System.Text.Encoding.Default)
        sw.WriteLine(RichTextBox1.Text)
        sw.Close()

        '--R2
        fileStream = New FileStream(Path + "\R2.txt", FileMode.OpenOrCreate)
        fileStream.Close()
        sw = New StreamWriter(Path + "\R2.txt", False, System.Text.Encoding.Default)
        sw.WriteLine(RichTextBox2.Text)
        sw.Close()

        '--R3
        fileStream = New FileStream(Path + "\R3.txt", FileMode.OpenOrCreate)
        fileStream.Close()
        sw = New StreamWriter(Path + "\R3.txt", False, System.Text.Encoding.Default)
        sw.WriteLine(RichTextBox3.Text)
        sw.Close()

        '--R4
        fileStream = New FileStream(Path + "\R4.txt", FileMode.OpenOrCreate)
        fileStream.Close()
        sw = New StreamWriter(Path + "\R4.txt", False, System.Text.Encoding.Default)
        sw.WriteLine(RichTextBox4.Text)
        sw.Close()

        '--R5
        fileStream = New FileStream(Path + "\R5.txt", FileMode.OpenOrCreate)
        fileStream.Close()
        sw = New StreamWriter(Path + "\R5.txt", False, System.Text.Encoding.Default)
        sw.WriteLine(RichTextBox5.Text)
        sw.Close()

        MessageBox.Show("寫入完成!!")
    End Sub

    ''' <summary>
    ''' reload
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button4_Click_1(sender As Object, e As EventArgs) Handles Button4.Click
        ini()
    End Sub

#Region "FUN"
    ''' <summary>
    ''' txt轉DT
    ''' </summary>
    ''' <param name="File"></param>
    ''' <param name="TableName"></param>
    ''' <param name="delimiter"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TxtConvertToDataTable(ByVal File As String, ByVal TableName As String, ByVal delimiter As String) As DataTable
        Dim dt As DataTable = New DataTable()
        Dim ds As DataSet = New DataSet()
        Dim s As StreamReader = New StreamReader(File, System.Text.Encoding.[Default])
        Dim columns As String() = s.ReadLine().Split(delimiter.ToCharArray())
        ds.Tables.Add(TableName)

        For Each col As String In columns
            Dim added As Boolean = False
            Dim [next] As String = ""
            Dim i As Integer = 0

            While Not added
                Dim columnname As String = col & [next]
                columnname = columnname.Replace("#", "")
                columnname = columnname.Replace("'", "")
                columnname = columnname.Replace("&", "")

                If Not ds.Tables(TableName).Columns.Contains(columnname) Then
                    ds.Tables(TableName).Columns.Add(columnname.ToUpper())
                    added = True
                Else
                    i += 1
                    [next] = "_" & i.ToString()
                End If
            End While
        Next

        Dim AllData As String = s.ReadToEnd()
        Dim rows As String() = AllData.Split(vbLf.ToCharArray())

        For Each r As String In rows
            Dim items As String() = r.Split(delimiter.ToCharArray())
            ds.Tables(TableName).Rows.Add(items)
        Next

        s.Close()
        dt = ds.Tables(0)
        dt.PrimaryKey = New DataColumn() {dt.Columns(0)} '設定主KEY,供SELECT用
        Return dt
    End Function

    Function replaceMail(Input As String) As String
        Input = Input.Replace(vbLf, "")

        Input = Input.Replace("{需求單}", txt需求單.Text)
        Input = Input.Replace("{換版單}", txt換版單.Text)
        Input = Input.Replace("{換版日}", dt換版日.Text)
        Input = Input.Replace("{TFS_SN}", txtTSF_SN.Text)
        Input = Input.Replace("{專案}", cmbPROJ.SelectedItem)

        Dim str As String = ""

        str = cmbSASD.SelectedItem.ToString
        Input = Input.Replace("{AP2}", str.Substring(str.Length - 2, 2)) '名字取最後兩個字

        str = cmbOK.SelectedItem.ToString
        Input = Input.Replace("{核准}", str.Substring(str.Length - 2, 2))

        str = cmbUSER.SelectedItem.ToString
        Input = Input.Replace("{USER}", str.Substring(str.Length - 2, 2))

        Return Input
    End Function
#End Region


    'Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
    '    Dim m_subject As String = "TFS換版:" & txt需求單.Text & "/" & txt換版單.Text
    '    Dim m_reciver As String = ""
    '    Dim m_body As String = ""

    '    '-------------------
    '    '格式說明:
    '    '空格 %20
    '    '逗號 %2C
    '    '問號 %3F
    '    '句號 %2E
    '    '驚嘆號 %21
    '    '冒號 %3a
    '    '分號 %3b
    '    '換行 %0a
    '    '-------------------

    '    '[SIT]
    '    m_reciver = cmbSASD.SelectedItem.ToString
    '    m_body = "Dear%20{AP2}：%0a%0a" & _
    '        "需求單：{需求單}%0a" & _
    '        "換版單：{換版單}%0a" & _
    '        "TFS序號：{TFS_SN}%0a%0a" & _
    '        "請協助AP2過版%0a" & _
    '        "請於TFS查詢管理系統(http%3a//10.11.1.60/)核准發行%0a" & _
    '        "專案名稱：{專案}%0a" & _
    '        "並交付%20{USER}%20驗收%0a"

    '    m_body = replaceMail(m_body)
    '    MAIL(m_subject, m_reciver, m_body)

    '    '[UAT]
    '    m_reciver = cmbUSER.SelectedItem.ToString
    '    m_body = "Dear%20{USER}：%0a%0a" & _
    '        "需求單：{需求單}%0a" & _
    '        "換版單：{換版單}%0a%0a" & _
    '        "於需求單系統，請用附件UAT文件驗收，感謝%21%21%0a" & _
    '        "預計{換版日}換版"

    '    m_body = replaceMail(m_body)
    '    MAIL(m_subject, m_reciver, m_body)

    '    '[TFS]
    '    m_reciver = cmbOK.SelectedItem.ToString
    '    m_body = "Dear%20{核准}：%0a%0a" & _
    '        "請於TFS查詢管理系統(http://10.11.1.60/)核准發行%0a%0a" & _
    '        "TFS序號：{TFS_SN}%0a" & _
    '        "專案代號：{專案}%0a" & _
    '        "換版單號：{換版單}%0a"

    '    m_body = replaceMail(m_body)
    '    MAIL(m_subject, m_reciver, m_body)

    '    '[TFS]
    '    m_reciver = "朱庭逸;余英偉;楊博富;林祺為"
    '    m_body = "Dear%20長官：%0a%0a" & _
    '        "{換版日}過版清單，麻煩您囉%21%21%0a%0a" & _
    '        "{專案}%20TFS過版%0a" & _
    '        "%20。換版單號：{換版單}%0a" & _
    '        "%20。TFS序號：{TFS_SN}"

    '    m_body = replaceMail(m_body)
    '    MAIL(m_subject, m_reciver, m_body)

    'End Sub

    ''' <summary>
    ''' 開啟TXT檔編輯
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click, Label4.Click, Label7.Click
        Process.Start("notepad", Path + "\group.txt")
    End Sub
End Class

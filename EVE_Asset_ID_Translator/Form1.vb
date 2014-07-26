Imports System.IO
Imports System.Xml


Public Class Form1
    Dim listAssetsTypeID2 As New List(Of XElement)
    Dim listTypeIDNumber As New List(Of Integer)
    Dim listTypeIDString As New List(Of String)
    Dim listStationNumber As New List(Of Integer)
    Dim listStationString As New List(Of String)
    Dim listSolarSystemID As New List(Of Integer)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadAssetList()
        LoadStationList()
        LoadTypeIDList()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        CreateHTML()
    End Sub

    Public Sub LoadAssetList()
        Dim xmlAssetList As XElement = XElement.Load("AssetList.xml")

        listAssetsTypeID2 = xmlAssetList...<row>.ToList()
    End Sub

    Public Sub LoadTypeIDList()
        Dim TypeIDReader As New StreamReader("Kronos_98534_typeID.csv")
        Dim arrayBuffer(1) As String

        While Not TypeIDReader.EndOfStream()
            arrayBuffer = TypeIDReader.ReadLine().Split(",")
            listTypeIDNumber.Add(CInt(arrayBuffer(0)))
            listTypeIDString.Add(arrayBuffer(1))
        End While

        TypeIDReader.Close()
    End Sub

    Public Sub LoadStationList()
        Dim StationsReader As New StreamReader("stations.csv")
        Dim arrayBuffer(13) As String

        ' the first line of the stations database is a descriptive header and its data is not needed
        StationsReader.ReadLine()

        While Not StationsReader.EndOfStream()
            arrayBuffer = StationsReader.ReadLine().Split(",")
            listStationNumber.Add(CInt(arrayBuffer(0)))
            listStationString.Add(arrayBuffer(11))
            listSolarSystemID.Add(CInt(arrayBuffer(3)))
        End While

        StationsReader.Close()
    End Sub

    Public Function BinarySearch(ByVal typeID As Integer) As Integer
        Dim index As Integer = 0
        Dim left = 0, right = listTypeIDNumber.Count - 1, mid As Integer
        mid = (left + right) / 2

        While Not left > right
            If typeID > listTypeIDNumber.Item(mid) Then
                left = mid + 1
                mid = (left + right) / 2
            ElseIf typeID < listTypeIDNumber(mid) Then
                right = mid - 1
                mid = (left + right) / 2
            ElseIf typeID = listTypeIDNumber.Item(mid) Then
                Return mid
            End If
        End While

        Return -1
    End Function

    Public Sub CreateHTML()
        Dim position As Integer = 0
        Dim strBuffer As String = Nothing
        Dim unknowns As Integer = 0
        Dim writer As New StreamWriter("AssetList.html")
        Dim currentLocationID As Integer = 0
        Dim classAttr As String = " class='i'"
        Dim itemLocationID As Integer = 0
        Dim stationIndex As Integer = 0

        writer.WriteLine("<!DOCTYPE html><html><head><link rel='stylesheet' href='style.css'><meta charset='UTF-8'></head><body>")

        For count As Integer = 0 To listAssetsTypeID2.Count - 1
            Try
                itemLocationID = CInt(listAssetsTypeID2.Item(count).@locationID)
                If (itemLocationID <> Nothing) And (currentLocationID <> itemLocationID) Then

                    currentLocationID = itemLocationID

                    If count <> 0 Then
                        writer.WriteLine("</table>")
                    End If

                    ' determines if the item is at a station or in space
                    If currentLocationID > 60000000 Then
                        stationIndex = listStationNumber.IndexOf(itemLocationID)
                        writer.WriteLine("<table><thead><tr><td colspan='2'>" & listStationString.Item(stationIndex) & "</td></tr></thead>")
                    ElseIf currentLocationID > 30000000 Then
                        stationIndex = listSolarSystemID.IndexOf(itemLocationID)
                        writer.WriteLine("<table><thead><tr><td colspan='2' class='s'>" & listStationString.Item(stationIndex) & " (in space)</td></tr></thead>")
                    End If

                End If
            Catch ex As Exception

            End Try

            position = CInt(BinarySearch(listAssetsTypeID2.Item(count).@typeID.ToString()))

            If position <> -1 Then

                If itemLocationID = Nothing Then
                    writer.WriteLine("<tr><td" & classAttr & ">" & listAssetsTypeID2.Item(count).@quantity.ToString() & "</td><td" & classAttr & ">" &
                                     listTypeIDString(position) & "</td></tr>")
                Else
                    writer.WriteLine("<tr><td>" & listAssetsTypeID2.Item(count).@quantity.ToString() & "</td><td>" &
                                     listTypeIDString(position) & "</td></tr>")
                End If
            Else
                unknowns += 1
                rtbUnknowns.Text = listAssetsTypeID2.Item(count).@typeID.ToString() & vbNewLine &
                                   listStationString.Item(stationIndex).ToString() & vbNewLine
            End If

            lblStatus.Text = "Complete. Unknown objects: " & unknowns
        Next

        writer.WriteLine("</table></body></html>")
        writer.Close()
    End Sub
End Class

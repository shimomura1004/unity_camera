using UnityEngine;
using System.Net.Sockets;

public class TexturePublisher : MonoBehaviour
{
    public int m_width = 640;
    public int m_height = 480;
    public int m_port = 8080;

    private Camera m_camera;
    private RenderTexture m_renderTexture;
    private Texture2D m_texture;
    private Rect m_rect;

    private string ip = "0.0.0.0";
    private TcpListener m_listener = null;
    private TcpClient m_client = null;
    private NetworkStream m_networkStream = null;
    private byte[] m_colorBuffer;

    private void StartTcpServer()
    {
        System.Net.IPAddress address = System.Net.IPAddress.Parse(ip);
        m_listener = new TcpListener(address, m_port);

        m_listener.Start();
        Debug.LogFormat("Listening on {0}:{1}", address, m_port);
    }

    private void AcceptPendingConnection()
    {
        m_client = m_listener.AcceptTcpClient();

        System.Net.IPEndPoint endpoint = (System.Net.IPEndPoint)m_client.Client.RemoteEndPoint;
        Debug.LogFormat("Accepted client: {0}:{1}", endpoint.Address, endpoint.Port);

        m_networkStream = m_client.GetStream();
        m_networkStream.ReadTimeout = 1 * 1000;
        m_networkStream.WriteTimeout = 1 * 10000;
    }

    private void UpdateTexture()
    {
        RenderTexture.active = m_camera.targetTexture;
        m_camera.Render();

        m_texture.ReadPixels(m_rect, 0, 0);
        m_texture.Apply();
    }

    private void SendBuffer(Color32[] color32)
    {
        if (m_networkStream.CanWrite)
        {
            for (int i = 0; i < m_width * m_height; i++)
            {
                m_colorBuffer[i * 3 + 0] = color32[i].r;
                m_colorBuffer[i * 3 + 1] = color32[i].g;
                m_colorBuffer[i * 3 + 2] = color32[i].b;
            }

            m_networkStream.Write(m_colorBuffer, 0, m_width * m_height * 3);
        }
    }

    void Start()
    {
        m_camera = GetComponent<Camera>();

        m_renderTexture = new RenderTexture(m_width, m_height, 0);
        m_camera.targetTexture = m_renderTexture;

        m_texture = new Texture2D(m_width, m_height);
        m_rect = new Rect(0, 0, m_width, m_height);

        m_colorBuffer = new byte[m_width * m_height * 3];

        StartTcpServer();
    }

    void Update()
    {
        if (m_client == null)
        {
            if (m_listener.Pending())
            {
                AcceptPendingConnection();
            }
        }
        else
        {
            if (m_client.Connected)
            {
                UpdateTexture();
                SendBuffer(m_texture.GetPixels32());
            }
            else
            {
                Debug.Log("Client has been disconnected");
                m_client = null;
                m_networkStream.Close();
                m_networkStream = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (m_networkStream != null)
        {
            m_networkStream.Close();
        }

        if (m_client != null)
        {
            m_client.Close();
        }

        m_listener.Stop();
    }
}

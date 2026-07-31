using System;
using System.Threading;
using System.Windows.Forms;

namespace repositorioo
{
    public class Form1 : Form
    {
        private CancellationTokenSource _cts;
        private bool _verificacaoAtiva = false;
        private MemoryEngine engine = new MemoryEngine();
        private System.Windows.Forms.Timer loopTimer;

        private bool teclaPressionadaAnteriormente = false;

        private TextBox txtElmoAtual, txtCapaAtual, txtOrnAtual;
        private TextBox txtDefElmo, txtDefCapa, txtDefOrn;
        private TextBox txtAtkElmo, txtAtkCapa, txtAtkOrn;
        private TextBox txtTeclaAtalho;
        private Label lblStatus;
        private Button btnConectar;
        private Button btnAtualizarItems;
        private Button btnAtualizarTroca;
        private Button BtnTestar;

        private ulong DefElmoInt;
        private ulong DefCapaInt;
        private ulong DefOrnInt;
        private ulong AtkElmoInt;
        private ulong AtkCapaInt;
        private ulong AtkOrnInt;

        public Form1()
        {
            ConfigurarInterface();
            ConfigurarTimer();

            try
            {
                string caminhoIcone = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Properties", "faviicon.ico");
                if (System.IO.File.Exists(caminhoIcone)) this.Icon = new System.Drawing.Icon(caminhoIcone);
                else this.Icon = new System.Drawing.Icon("faviicon.ico");

                // Se for a primeira vez abrindo, ele vai usar os valores padrões que você definiu na tabela
                txtTeclaAtalho.Text = Properties.Settings.Default.TeclaAtalho;

                txtAtkElmo.Text = Properties.Settings.Default.AtkElmo;
                txtAtkCapa.Text = Properties.Settings.Default.AtkCapa;
                txtAtkOrn.Text = Properties.Settings.Default.AtkOrn;

                txtDefElmo.Text = Properties.Settings.Default.DefElmo;
                txtDefCapa.Text = Properties.Settings.Default.DefCapa;
                txtDefOrn.Text = Properties.Settings.Default.DefOrn;

                // Processa a conversão inicial dos IDs para a macro funcionar de imediato
                AtualizarTroca(null, null);

                // Processa a conversão inicial dos IDs para a macro funcionar de imediato
                AtualizarTroca(null, null);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Erro ao carregar o ícone: " + ex.Message); }

            this.FormClosing += (s, e) => { engine.DesconectarEParar(); loopTimer?.Stop(); Environment.Exit(0); };
        }

        private void ConfigurarInterface()
        {
            this.Text = "Repositorio";
            this.Size = new System.Drawing.Size(165, 420);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblAtual = new Label() { Text = "ITEM ATUAL", Top = 10, Left = 10, Width = 250, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold) };
            txtElmoAtual = new TextBox() { Text = "", Top = 35, Left = 10, Width = 40, ReadOnly = true };
            txtCapaAtual = new TextBox() { Text = "", Top = 35, Left = 60, Width = 40, ReadOnly = true };
            txtOrnAtual = new TextBox() { Text = "", Top = 35, Left = 105, Width = 40, ReadOnly = true };

            Label lblAtk = new Label() { Text = "SET (ATAQUE)", Top = 75, Left = 10, Width = 250, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold) };
            txtAtkElmo = new TextBox() { Text = "", Top = 100, Left = 10, Width = 40 };
            txtAtkCapa = new TextBox() { Text = "", Top = 100, Left = 60, Width = 40 };
            txtAtkOrn = new TextBox() { Text = "", Top = 100, Left = 105, Width = 40 };

            Label lblDef = new Label() { Text = "SET (DEFESA)", Top = 145, Left = 10, Width = 250, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold) };
            txtDefElmo = new TextBox() { Text = "", Top = 165, Left = 10, Width = 40 };
            txtDefCapa = new TextBox() { Text = "", Top = 165, Left = 60, Width = 40 };
            txtDefOrn = new TextBox() { Text = "", Top = 165, Left = 105, Width = 40 };

            Label lblAtalho = new Label() { Text = "ATALHO:", Top = 200, Left = 50, Width = 55, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold) };
            txtTeclaAtalho = new TextBox() { Text = "", Top = 225, Left = 62, Width = 25};

            btnConectar = new Button() { Text = "Conectar", Top = 260, Left = 8, Width = 135, Height = 30 };
            btnConectar.Click += BtnConectar_Click;

            btnAtualizarItems = new Button() { Text = "Item atual", Top = 295, Left = 8, Width = 135, Height = 30 };
            btnAtualizarItems.Click += atualizarItens;

            btnAtualizarTroca = new Button() { Text = "Atualizar", Top = 330, Left = 8, Width = 135, Height = 30 };
            btnAtualizarTroca.Click += AtualizarTroca;

            //BtnTestar = new Button() { Text = "Testar", Top = 370, Left = 8, Width = 135, Height = 30 };
            //BtnTestar.Click += BtnTestarFuncao_Click;

            lblStatus = new Label() { Text = "Aguardando conexão", Top = 360, Left = 18, Width = 220, ForeColor = System.Drawing.Color.Red };

            this.Controls.AddRange(new Control[] {
                lblAtual, txtElmoAtual, txtCapaAtual, txtOrnAtual,
                lblAtk, txtAtkElmo, txtAtkCapa, txtAtkOrn,
                lblDef, txtDefElmo, txtDefCapa, txtDefOrn,
                lblAtalho, txtTeclaAtalho, btnConectar, lblStatus, btnAtualizarItems, btnAtualizarTroca,BtnTestar,
            });

            // Força a leitura inicial dos IDs ao abrir o programa baseado nas caixas padrões
            AtualizarTroca(null, null);
        }

        private void atualizarItens(object sender, EventArgs e)
        {
            try
            {
                if (engine.HProcess != IntPtr.Zero)
                {
                    engine.RelerItensAtuais();
                    txtElmoAtual.Text = engine.elmoAtual.ToString("X");
                    txtCapaAtual.Text = engine.capaAtual.ToString("X");
                    txtOrnAtual.Text = engine.ornAtual.ToString("X");
                }
            }
            catch { }
        }

        private void Azure() { } // Método fantasma removido implicitamente

        private void Hex() { } // Método fantasma removido implicitamente

        private void AtualizarTroca(object sender, EventArgs e)
        {
            try
            {
                // Salva as variáveis numéricas ulong na memória ram do app
                DefElmoInt = Convert.ToUInt32(txtDefElmo.Text, 16);
                DefCapaInt = Convert.ToUInt32(txtDefCapa.Text, 16);
                DefOrnInt = Convert.ToUInt32(txtDefOrn.Text, 16);

                AtkElmoInt = Convert.ToUInt32(txtAtkElmo.Text, 16);
                AtkCapaInt = Convert.ToUInt32(txtAtkCapa.Text, 16);
                AtkOrnInt = Convert.ToUInt32(txtAtkOrn.Text, 16);

                // --- GRAVAR OS TEXTOS DIRETOS NAS CONFIGURAÇÕES NATIVAS ---
                Properties.Settings.Default.TeclaAtalho = txtTeclaAtalho.Text;

                Properties.Settings.Default.AtkElmo = txtAtkElmo.Text;
                Properties.Settings.Default.AtkCapa = txtAtkCapa.Text;
                Properties.Settings.Default.AtkOrn = txtAtkOrn.Text;

                Properties.Settings.Default.DefElmo = txtDefElmo.Text;
                Properties.Settings.Default.DefCapa = txtDefCapa.Text;
                Properties.Settings.Default.DefOrn = txtDefOrn.Text;

                // Commita as alterações fisicamente no arquivo do computador
                Properties.Settings.Default.Save();

                if (sender != null)
                {
                    MessageBox.Show("Salvo","", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                if (sender != null)
                {
                    MessageBox.Show("Erro ao converter algum ID. Verifique o texto digitado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ConfigurarTimer()
        {
            loopTimer = new System.Windows.Forms.Timer();
            loopTimer.Interval = 30;
            loopTimer.Tick += LoopTimer_Tick;
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            if (!engine.Conectar())
            {
                lblStatus.Text = "Jogo não encontrado";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }
            lblStatus.Text = "Conectado";
            lblStatus.ForeColor = System.Drawing.Color.Green;
            loopTimer.Start();
        }

        private void BtnTestarFuncao_Click(object sender, EventArgs e)
        {
            if (engine.HProcess == IntPtr.Zero) return;
            try { engine.teste2(0xB0B0E9C0,0x2F537F80,0x180468EE0,0x28807CB0); } catch { }
        }

        private void InitializeComponent() { }

        private void LoopTimer_Tick(object sender, EventArgs e)
        {
            if (engine.HProcess == IntPtr.Zero) return;

            int codigoTeclaValido = 0x58;
            try
            {
                Keys teclaConvertida = (Keys)Enum.Parse(typeof(Keys), txtTeclaAtalho.Text, true);
                codigoTeclaValido = (int)teclaConvertida;
            }
            catch { }

            bool teclaEstaApertadaAgora = (MemoryEngine.GetAsyncKeyState(codigoTeclaValido) & 0x8000) != 0;

            if (teclaEstaApertadaAgora)
            {
                if (!teclaPressionadaAnteriormente)
                {
                    teclaPressionadaAnteriormente = true;
                    loopTimer.Stop();

                    try
                    {

                        engine.ProcessarTrocaDeSet(DefElmoInt, DefCapaInt, DefOrnInt, AtkElmoInt, AtkCapaInt, AtkOrnInt);

                    }
                    catch
                    {
                        System.Media.SystemSounds.Beep.Play();
                    }

                    loopTimer.Start();
                }
            }
            else
            {
                teclaPressionadaAnteriormente = false;
            }
        }
    }
}
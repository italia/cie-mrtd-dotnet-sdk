namespace Test
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.nome = new System.Windows.Forms.TextBox();
            this.cognome = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.dataNascita = new System.Windows.Forms.TextBox();
            this.text45 = new System.Windows.Forms.TextBox();
            this.sesso = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.indirizzo = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.statura = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.cittadinanza = new System.Windows.Forms.TextBox();
            this.cf = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.rilascio = new System.Windows.Forms.TextBox();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.mrz = new System.Windows.Forms.TextBox();
            this.luogoNascita = new System.Windows.Forms.TextBox();
            this.trtrtr = new System.Windows.Forms.TextBox();
            this.provinciaNascita = new System.Windows.Forms.TextBox();
            this.provincia = new System.Windows.Forms.TextBox();
            this.city = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.status_ = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.radioButtonXml = new System.Windows.Forms.RadioButton();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.groupBoxRadio = new System.Windows.Forms.GroupBox();
            this.radioButtonCSV = new System.Windows.Forms.RadioButton();
            this.radioButtonJSon = new System.Windows.Forms.RadioButton();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonVerifica = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.groupBoxRadio.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 0;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(40, 4);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(162, 184);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            this.pictureBox.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // nome
            // 
            this.nome.BackColor = System.Drawing.SystemColors.Window;
            this.nome.Location = new System.Drawing.Point(317, 22);
            this.nome.Name = "nome";
            this.nome.ReadOnly = true;
            this.nome.Size = new System.Drawing.Size(80, 20);
            this.nome.TabIndex = 3;
            this.nome.Text = "//";
            // 
            // cognome
            // 
            this.cognome.BackColor = System.Drawing.SystemColors.Window;
            this.cognome.Location = new System.Drawing.Point(475, 22);
            this.cognome.Name = "cognome";
            this.cognome.ReadOnly = true;
            this.cognome.Size = new System.Drawing.Size(80, 20);
            this.cognome.TabIndex = 5;
            this.cognome.Text = "//";
            this.cognome.TextChanged += new System.EventHandler(this.cognome_TextChanged);
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Location = new System.Drawing.Point(416, 25);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(53, 13);
            this.textBox3.TabIndex = 4;
            this.textBox3.Text = "Cognome:";
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(260, 63);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(70, 13);
            this.textBox2.TabIndex = 6;
            this.textBox2.Text = "Data Nascita:";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // dataNascita
            // 
            this.dataNascita.BackColor = System.Drawing.SystemColors.Window;
            this.dataNascita.Location = new System.Drawing.Point(327, 60);
            this.dataNascita.Name = "dataNascita";
            this.dataNascita.ReadOnly = true;
            this.dataNascita.Size = new System.Drawing.Size(61, 20);
            this.dataNascita.TabIndex = 7;
            this.dataNascita.Text = "//";
            // 
            // text45
            // 
            this.text45.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.text45.Location = new System.Drawing.Point(576, 25);
            this.text45.Name = "text45";
            this.text45.ReadOnly = true;
            this.text45.Size = new System.Drawing.Size(41, 13);
            this.text45.TabIndex = 8;
            this.text45.Text = "Sesso:";
            this.text45.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // sesso
            // 
            this.sesso.BackColor = System.Drawing.SystemColors.Window;
            this.sesso.Location = new System.Drawing.Point(611, 22);
            this.sesso.Name = "sesso";
            this.sesso.ReadOnly = true;
            this.sesso.Size = new System.Drawing.Size(25, 20);
            this.sesso.TabIndex = 9;
            this.sesso.Text = "//";
            // 
            // textBox4
            // 
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Location = new System.Drawing.Point(260, 168);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(51, 13);
            this.textBox4.TabIndex = 10;
            this.textBox4.Text = "Indirizzo:";
            this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged_1);
            // 
            // indirizzo
            // 
            this.indirizzo.BackColor = System.Drawing.SystemColors.Window;
            this.indirizzo.Location = new System.Drawing.Point(317, 168);
            this.indirizzo.Name = "indirizzo";
            this.indirizzo.ReadOnly = true;
            this.indirizzo.Size = new System.Drawing.Size(285, 20);
            this.indirizzo.TabIndex = 11;
            this.indirizzo.Text = "//";
            this.indirizzo.TextChanged += new System.EventHandler(this.textBox5_TextChanged);
            // 
            // textBox5
            // 
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Location = new System.Drawing.Point(518, 63);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(51, 13);
            this.textBox5.TabIndex = 12;
            this.textBox5.Text = "Statura:";
            this.textBox5.TextChanged += new System.EventHandler(this.textBox5_TextChanged_1);
            // 
            // statura
            // 
            this.statura.BackColor = System.Drawing.SystemColors.Window;
            this.statura.Location = new System.Drawing.Point(562, 60);
            this.statura.Name = "statura";
            this.statura.ReadOnly = true;
            this.statura.Size = new System.Drawing.Size(40, 20);
            this.statura.TabIndex = 13;
            this.statura.Text = "//";
            // 
            // textBox6
            // 
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.Location = new System.Drawing.Point(402, 63);
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.Size = new System.Drawing.Size(67, 13);
            this.textBox6.TabIndex = 14;
            this.textBox6.Text = "Cittadinanza:";
            this.textBox6.TextChanged += new System.EventHandler(this.textBox6_TextChanged);
            // 
            // cittadinanza
            // 
            this.cittadinanza.BackColor = System.Drawing.SystemColors.Window;
            this.cittadinanza.Location = new System.Drawing.Point(475, 60);
            this.cittadinanza.Name = "cittadinanza";
            this.cittadinanza.ReadOnly = true;
            this.cittadinanza.Size = new System.Drawing.Size(37, 20);
            this.cittadinanza.TabIndex = 15;
            this.cittadinanza.Text = "//";
            this.cittadinanza.TextChanged += new System.EventHandler(this.textBox7_TextChanged);
            // 
            // cf
            // 
            this.cf.BackColor = System.Drawing.SystemColors.Window;
            this.cf.Location = new System.Drawing.Point(291, 133);
            this.cf.Name = "cf";
            this.cf.ReadOnly = true;
            this.cf.Size = new System.Drawing.Size(264, 20);
            this.cf.TabIndex = 17;
            this.cf.Text = "//";
            // 
            // textBox8
            // 
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Location = new System.Drawing.Point(260, 136);
            this.textBox8.Name = "textBox8";
            this.textBox8.ReadOnly = true;
            this.textBox8.Size = new System.Drawing.Size(25, 13);
            this.textBox8.TabIndex = 16;
            this.textBox8.Text = "CF:";
            // 
            // rilascio
            // 
            this.rilascio.BackColor = System.Drawing.SystemColors.Window;
            this.rilascio.Location = new System.Drawing.Point(336, 236);
            this.rilascio.Name = "rilascio";
            this.rilascio.ReadOnly = true;
            this.rilascio.Size = new System.Drawing.Size(69, 20);
            this.rilascio.TabIndex = 19;
            this.rilascio.Text = "//";
            // 
            // textBox9
            // 
            this.textBox9.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox9.Location = new System.Drawing.Point(260, 239);
            this.textBox9.Name = "textBox9";
            this.textBox9.ReadOnly = true;
            this.textBox9.Size = new System.Drawing.Size(73, 13);
            this.textBox9.TabIndex = 18;
            this.textBox9.Text = "Data Rilascio: ";
            this.textBox9.TextChanged += new System.EventHandler(this.textBox9_TextChanged);
            // 
            // mrz
            // 
            this.mrz.BackColor = System.Drawing.SystemColors.Window;
            this.mrz.Location = new System.Drawing.Point(36, 194);
            this.mrz.Multiline = true;
            this.mrz.Name = "mrz";
            this.mrz.ReadOnly = true;
            this.mrz.Size = new System.Drawing.Size(166, 88);
            this.mrz.TabIndex = 21;
            this.mrz.Text = "//";
            // 
            // luogoNascita
            // 
            this.luogoNascita.BackColor = System.Drawing.SystemColors.Window;
            this.luogoNascita.Location = new System.Drawing.Point(339, 101);
            this.luogoNascita.Name = "luogoNascita";
            this.luogoNascita.ReadOnly = true;
            this.luogoNascita.Size = new System.Drawing.Size(270, 20);
            this.luogoNascita.TabIndex = 25;
            this.luogoNascita.Text = "//";
            // 
            // trtrtr
            // 
            this.trtrtr.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.trtrtr.Location = new System.Drawing.Point(260, 104);
            this.trtrtr.Name = "trtrtr";
            this.trtrtr.ReadOnly = true;
            this.trtrtr.Size = new System.Drawing.Size(73, 13);
            this.trtrtr.TabIndex = 24;
            this.trtrtr.Text = "Luogo Nascita:";
            // 
            // provinciaNascita
            // 
            this.provinciaNascita.BackColor = System.Drawing.SystemColors.Window;
            this.provinciaNascita.Location = new System.Drawing.Point(615, 101);
            this.provinciaNascita.Name = "provinciaNascita";
            this.provinciaNascita.ReadOnly = true;
            this.provinciaNascita.Size = new System.Drawing.Size(31, 20);
            this.provinciaNascita.TabIndex = 26;
            this.provinciaNascita.Text = "//";
            // 
            // provincia
            // 
            this.provincia.BackColor = System.Drawing.SystemColors.Window;
            this.provincia.Location = new System.Drawing.Point(444, 192);
            this.provincia.Name = "provincia";
            this.provincia.ReadOnly = true;
            this.provincia.Size = new System.Drawing.Size(31, 20);
            this.provincia.TabIndex = 27;
            this.provincia.Text = "//";
            // 
            // city
            // 
            this.city.BackColor = System.Drawing.SystemColors.Window;
            this.city.Location = new System.Drawing.Point(317, 192);
            this.city.Name = "city";
            this.city.ReadOnly = true;
            this.city.Size = new System.Drawing.Size(121, 20);
            this.city.TabIndex = 28;
            this.city.Text = "//";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(61, 4);
            // 
            // status_
            // 
            this.status_.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.status_.Location = new System.Drawing.Point(416, 377);
            this.status_.Name = "status_";
            this.status_.ReadOnly = true;
            this.status_.Size = new System.Drawing.Size(219, 13);
            this.status_.TabIndex = 31;
            this.status_.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(273, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(38, 13);
            this.textBox1.TabIndex = 32;
            this.textBox1.Text = "Nome:";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            // 
            // textBox10
            // 
            this.textBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox10.Location = new System.Drawing.Point(2, 197);
            this.textBox10.Name = "textBox10";
            this.textBox10.ReadOnly = true;
            this.textBox10.Size = new System.Drawing.Size(32, 13);
            this.textBox10.TabIndex = 33;
            this.textBox10.Text = "MRZ:";
            // 
            // radioButtonXml
            // 
            this.radioButtonXml.AutoSize = true;
            this.radioButtonXml.Location = new System.Drawing.Point(15, 19);
            this.radioButtonXml.Name = "radioButtonXml";
            this.radioButtonXml.Size = new System.Drawing.Size(47, 17);
            this.radioButtonXml.TabIndex = 34;
            this.radioButtonXml.TabStop = true;
            this.radioButtonXml.Text = "XML";
            this.radioButtonXml.UseVisualStyleBackColor = true;
            // 
            // textBox7
            // 
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Location = new System.Drawing.Point(24, 300);
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            this.textBox7.Size = new System.Drawing.Size(73, 13);
            this.textBox7.TabIndex = 35;
            this.textBox7.Text = "Export/Verifica:";
            // 
            // groupBoxRadio
            // 
            this.groupBoxRadio.Controls.Add(this.radioButtonJSon);
            this.groupBoxRadio.Controls.Add(this.radioButtonCSV);
            this.groupBoxRadio.Controls.Add(this.radioButtonXml);
            this.groupBoxRadio.Location = new System.Drawing.Point(128, 300);
            this.groupBoxRadio.Name = "groupBoxRadio";
            this.groupBoxRadio.Size = new System.Drawing.Size(310, 47);
            this.groupBoxRadio.TabIndex = 36;
            this.groupBoxRadio.TabStop = false;
            this.groupBoxRadio.Text = "Tipo Codifica";
            // 
            // radioButtonCSV
            // 
            this.radioButtonCSV.AutoSize = true;
            this.radioButtonCSV.Location = new System.Drawing.Point(117, 19);
            this.radioButtonCSV.Name = "radioButtonCSV";
            this.radioButtonCSV.Size = new System.Drawing.Size(46, 17);
            this.radioButtonCSV.TabIndex = 35;
            this.radioButtonCSV.TabStop = true;
            this.radioButtonCSV.Text = "CSV";
            this.radioButtonCSV.UseVisualStyleBackColor = true;
            // 
            // radioButtonJSon
            // 
            this.radioButtonJSon.AutoSize = true;
            this.radioButtonJSon.Location = new System.Drawing.Point(208, 19);
            this.radioButtonJSon.Name = "radioButtonJSon";
            this.radioButtonJSon.Size = new System.Drawing.Size(49, 17);
            this.radioButtonJSon.TabIndex = 36;
            this.radioButtonJSon.TabStop = true;
            this.radioButtonJSon.Text = "JSon";
            this.radioButtonJSon.UseVisualStyleBackColor = true;
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(128, 367);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 37;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            // 
            // buttonVerifica
            // 
            this.buttonVerifica.Location = new System.Drawing.Point(236, 367);
            this.buttonVerifica.Name = "buttonVerifica";
            this.buttonVerifica.Size = new System.Drawing.Size(75, 23);
            this.buttonVerifica.TabIndex = 38;
            this.buttonVerifica.Text = "Verifica";
            this.buttonVerifica.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 436);
            this.Controls.Add(this.buttonVerifica);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.groupBoxRadio);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox10);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.status_);
            this.Controls.Add(this.city);
            this.Controls.Add(this.provincia);
            this.Controls.Add(this.provinciaNascita);
            this.Controls.Add(this.luogoNascita);
            this.Controls.Add(this.trtrtr);
            this.Controls.Add(this.mrz);
            this.Controls.Add(this.rilascio);
            this.Controls.Add(this.textBox9);
            this.Controls.Add(this.cf);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.cittadinanza);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.statura);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.indirizzo);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.sesso);
            this.Controls.Add(this.text45);
            this.Controls.Add(this.dataNascita);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.cognome);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.nome);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "La Tua Carta d\'Identità Elettronica";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.groupBoxRadio.ResumeLayout(false);
            this.groupBoxRadio.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox text45;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.TextBox trtrtr;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        public System.Windows.Forms.TextBox status_;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox10;
        public System.Windows.Forms.TextBox nome;
        public System.Windows.Forms.TextBox cognome;
        public System.Windows.Forms.TextBox dataNascita;
        public System.Windows.Forms.TextBox sesso;
        public System.Windows.Forms.TextBox indirizzo;
        public System.Windows.Forms.TextBox statura;
        public System.Windows.Forms.TextBox cittadinanza;
        public System.Windows.Forms.TextBox cf;
        public System.Windows.Forms.TextBox rilascio;
        public System.Windows.Forms.TextBox mrz;
        public System.Windows.Forms.TextBox luogoNascita;
        public System.Windows.Forms.TextBox provinciaNascita;
        public System.Windows.Forms.TextBox provincia;
        public System.Windows.Forms.TextBox city;
        public System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox textBox7;
        public System.Windows.Forms.RadioButton radioButtonXml;
        public System.Windows.Forms.RadioButton radioButtonJSon;
        public System.Windows.Forms.RadioButton radioButtonCSV;
        public System.Windows.Forms.GroupBox groupBoxRadio;
        public System.Windows.Forms.Button buttonExport;
        public System.Windows.Forms.Button buttonVerifica;
    }
}


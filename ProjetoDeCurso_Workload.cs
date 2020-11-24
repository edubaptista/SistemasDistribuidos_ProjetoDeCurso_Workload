using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;

/*  
 *  Eduardo Baptista dos Santos
 *  Disciplina: CC7261 - Sistemas Distribuídos
 *  Professor: Ricardo Destro
 *  Centro Universitário FEI
 *  
 */

namespace ProjetoDeCurso_SD_Workload
{

    class Program
    {

        /*
        Para ler um arquivo e gerar uma saida em sua maquina local, 
        alterar o endereco da base. Ex: @"C:\Users\Eduardo\Desktop\BASE.txt"
        */

        //Caminho da base de entrada que será lida
        public static string caminhoBaseEntrada = @"BASE.txt";
        //Caminho das bases de saída que serão geradas
        public static string caminhoBaseSaida = @"C:\Users\Eduardo Baptista\Desktop\";
        //Declaração das listas que serão utilizadas para calcular os digitos dos CPFs e CNPJs
        public static List<string> listaCPFs, listaCNPJs, listaCPFsCompletos, listaCNPJsCompletos;
        //Declaração das variáveis para contagem dos tempos
        public static Stopwatch tempoLeituraBase, tempoCPF, tempoCNPJ, tempoTotalGasto;
        //Declaração do escritor de fluxo (usado para gerar a base de saída)
        public static StreamWriter streamWriter;

        public static void Main(string[] args)
        {
            //Componente orquestrador
            orquestrador();
        }

        public static void orquestrador()
        {
            tempoTotalGasto = new Stopwatch();
            tempoTotalGasto.Start();

            //Criação das listas para contagens e validações
            listaCPFs = new List<string>();
            listaCNPJs = new List<string>();
            listaCPFsCompletos = new List<string>();
            listaCNPJsCompletos = new List<string>();

            //Configuração dos cronômetros
            tempoLeituraBase = new Stopwatch();
            tempoCPF = new Stopwatch();
            tempoCNPJ = new Stopwatch();

            Console.WriteLine("Realizando a leitura da base...\n");

            //Chamada de procedimento para a leitura da base de CPFs e CNPJs
            leituraDaBase(caminhoBaseEntrada);

            Console.WriteLine("Base lida!\n");

            Console.WriteLine("Quantidade de CPFs lidos da base: {0} CPFs\n", listaCPFs.Count);
            Console.WriteLine("Quantidade de CNPJs lidos da base: {0} CNPJs\n", listaCNPJs.Count);
            Console.WriteLine("Quantidade total de dados lidos da base: {0} dados\n", listaCPFs.Count + listaCNPJs.Count);
            Console.WriteLine("Tempo gasto na leitura da base: {0}ms\n", tempoLeituraBase.ElapsedMilliseconds);

            /*
            geraDigitoCPF();
            geraDigitoCNPJ();
            */
            
            Console.WriteLine("Iniciando as threads.\n");

            //Declaração de nova thread para a funcao que gera os digitos de CPF
            Thread threadCPFs = new Thread(new ThreadStart(geraDigitoCPF));
            //Declaração de nova thread para a funcao que gera os digitos de CNPJ
            Thread threadCNPJs = new Thread(new ThreadStart(geraDigitoCNPJ));

            //Iniciando as threads
            threadCPFs.Start();
            threadCNPJs.Start();

            //Método join executa as threads
            threadCPFs.Join();
            threadCNPJs.Join();
            
            Console.WriteLine("Tempo gasto na thread para calcular digito dos CPFs: {0}ms\n", tempoCPF.ElapsedMilliseconds);
            Console.WriteLine("Tempo gasto na thread para calcular digito dos CNPJs: {0}ms\n", tempoCNPJ.ElapsedMilliseconds);
            Console.WriteLine("Tempo gasto durante toda a execução dos processos: {0}ms\n", tempoTotalGasto.ElapsedMilliseconds);


            geradorBases("baseCPFs.txt", listaCPFsCompletos, caminhoBaseSaida);
            geradorBases("baseCNPJs.txt", listaCNPJsCompletos, caminhoBaseSaida);


            System.Console.ReadLine();
            tempoTotalGasto.Stop();


            Console.WriteLine("Fim do processo.");

        }

        /* Procedimento que recebe a base lida para trata-los
         * Neste procedimento também são contados quantas linhas estao sendo lidas
         * Contador de quantidade 
        */
        public static void leituraDaBase(string baseDeDados)
        {
            tempoLeituraBase.Start();
            //int contadorTotal = 0;
            //int contadorCPF = 0;
            //int contadorCNPJ = 0;
            string dado;

            StreamReader baseDados = new StreamReader(baseDeDados);
            while ((dado = baseDados.ReadLine()) != null)
            {
                //Console.WriteLine("Lendo a linha {0}...", contadorTotal);
                
                string dadoTratado;
                dadoTratado = dado.TrimStart();
                if (dadoTratado.Length == 9)
                {
                    //contadorCPF++;
                    listaCPFs.Add(dadoTratado);
                }
                else
                {
                    //contadorCNPJ++;
                    listaCNPJs.Add(dadoTratado);
                }

                //contadorTotal++;
            }
            tempoLeituraBase.Stop();
            baseDados.Close();
        }

        public static void geradorBases(string nomeArquivo, List<String> lista, string enderecoSaida)
        {

            using (streamWriter = new StreamWriter(string.Concat(enderecoSaida, nomeArquivo)))
            {
                foreach (string linha in lista)
                {
                    streamWriter.WriteLine(linha);
                }
            }
            streamWriter.Close();
        }
        public static void geraDigitoCPF()
        {
            tempoCPF.Start();
            foreach (string cpf in listaCPFs)
            {
                string tempCPF = "";
                string digito = "";
                int auxiliar1, auxiliar2;

                //Retira carcteres invalidos não numericos da string

                //Ajusta o Tamanho do CPF para 9 digitos completando com zeros a esquerda
                tempCPF = Convert.ToInt64(cpf).ToString("D9");

                //Calcula o primeiro digito do CPF
                auxiliar1 = 0;

                for (int i = 0; i < tempCPF.Length; i++)
                {
                    auxiliar1 += Convert.ToInt32(tempCPF.Substring(i, 1)) * (10 - i);
                }

                auxiliar2 = 11 - (auxiliar1 % 11);

                //Carrega o primeiro digito na variavel digito
                if (auxiliar2 > 9)
                    digito += "0";
                else
                    digito += auxiliar2.ToString();

                //Adiciona o primeiro digito ao final do CPF para calculo do segundo digito
                tempCPF += digito;

                //Calcula o segundo digito do CPF
                auxiliar1 = 0;

                for (int i = 0; i < tempCPF.Length; i++)
                {
                    auxiliar1 += Convert.ToInt32(tempCPF.Substring(i, 1)) * (11 - i);
                }

                auxiliar2 = 11 - (auxiliar1 % 11);

                //Carrega o segundo digito na variavel digito
                if (auxiliar2 > 9)
                    digito += "0";
                else
                    digito += auxiliar2.ToString();

                string cpfCompleto = cpf + digito;

                listaCPFsCompletos.Add(cpfCompleto);
            }
            tempoCPF.Stop();
            //Console.WriteLine("Fim da analise CPF.");
        }
 
    public static void geraDigitoCNPJ()
        {
            tempoCNPJ.Start();
            foreach (string cnpj in listaCNPJs)
            {

                string tempCNPJ = "";
                string digito = "";
                int[] calculo = new int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int auxiliar1, auxiliar2;

                //Ajusta o Tamanho do CNPJ para 12 digitos completando com zeros a esquerda
                tempCNPJ = Convert.ToInt64(cnpj).ToString("D12");

                //Calcula o primeiro digito do CNPJ
                auxiliar1 = 0;

                for (int i = 0; i < tempCNPJ.Length; i++)
                {
                    auxiliar1 += Convert.ToInt32(tempCNPJ.Substring(i, 1)) * calculo[i];
                }

                auxiliar2 = (auxiliar1 % 11);

                //Carrega o primeiro digito na variavel digito
                if (auxiliar2 < 2)
                    digito += "0";
                else
                    digito += (11 - auxiliar2).ToString();

                //Adiciona o primeiro digito ao final do CNPJ para calculo do segundo digito
                tempCNPJ += digito;

                //Calcula o segundo digito do CNPJ
                calculo = new int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                auxiliar1 = 0;

                for (int i = 0; i < tempCNPJ.Length; i++)
                {
                    auxiliar1 += Convert.ToInt32(tempCNPJ.Substring(i, 1)) * calculo[i];
                }

                auxiliar2 = (auxiliar1 % 11);

                //Carrega o segundo digito na variavel digito
                if (auxiliar2 < 2)
                    digito += "0";
                else
                    digito += (11 - auxiliar2).ToString();

                string cnpjCompleto = cnpj + digito.ToString();

                listaCNPJsCompletos.Add(cnpjCompleto);
            }
            tempoCNPJ.Stop();
            //Console.WriteLine("Fim da analise CNPJ.");
        }
    }
}

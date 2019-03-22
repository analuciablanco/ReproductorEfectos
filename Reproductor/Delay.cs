using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Reproductor
{
    class Delay : ISampleProvider
    {
        private ISampleProvider fuente;
        private int offsetMilisegundos;

        public float ganancia;
        public int OffsetMilisegundos {
            get {
                return offsetMilisegundos;
            }
            set {
                offsetMilisegundos = value;
                cantidadMuestrasOffset = (int)(((float)(OffsetMilisegundos) / 1000.0f) * (float)fuente.WaveFormat.SampleRate);
            }
        }
        private int cantidadMuestrasOffset;

        private List<float> bufferDelay = new List<float>();

        private int tamanoBuffer;
        private int duracionBufferSegundos;

        private int cantidadMuestrasTranscurridas = 0;
        private int cantidadMuestrasBorradas = 0;

        public bool Activo { get; set; }

        public WaveFormat WaveFormat
        {
            get
            {
                return fuente.WaveFormat;
            }
        }

        public Delay(ISampleProvider fuente)
        {

            Activo = false;
            this.fuente = fuente;
            OffsetMilisegundos = 500;
            cantidadMuestrasOffset = (int)(((float)(OffsetMilisegundos)/1000.0f) * (float) fuente.WaveFormat.SampleRate);
            duracionBufferSegundos = 10;

            tamanoBuffer = fuente.WaveFormat.SampleRate * duracionBufferSegundos;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            // Leemos las muestras de la señal fuente.
            var read = fuente.Read(buffer, offset, count);
            // Calculamos el tiempo transcurrido.
            float tiempoSegundosTranscurridos = (float)cantidadMuestrasTranscurridas / (float)fuente.WaveFormat.SampleRate;
            float milisegundosTranscurridos = tiempoSegundosTranscurridos * 1000.0f;
            // Llenamos el buffer que creamos.
            for (int i = 0; i < read; i++)
            {
                bufferDelay.Add(buffer[i + offset]);
            }
            // Eliminamos excedentes del buffer.
            if (bufferDelay.Count > tamanoBuffer)
            {
                int diferencia = bufferDelay.Count - tamanoBuffer;
                bufferDelay.RemoveRange(0, diferencia);
                cantidadMuestrasBorradas += diferencia;
            }

            if (Activo) // Si el checkbox está activo realiza lo siguiente:
            {
                // Aplicamos el efecto.
                if (milisegundosTranscurridos > OffsetMilisegundos)
                {
                    for (int i = 0; i < read; i++)
                    {
                        buffer[offset + i] += bufferDelay[cantidadMuestrasTranscurridas - cantidadMuestrasBorradas + i - cantidadMuestrasOffset] * ganancia;
                    }
                }
            }

            cantidadMuestrasTranscurridas += read;
            return read;
        }
    }
}

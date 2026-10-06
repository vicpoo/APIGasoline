// PRUEBA DE PICOS: de 10 a 300 usuarios de golpe (ej. todos los operadores registran cargas al mismo tiempo).
// Pregunta que responde: ¿sobrevive a un pico repentino y se recupera después?
// k6 run pruebas-carga/4-picos.js
import { setup as setupComun, sesionDeUsuario, umbrales } from './comun.js';

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '10s', target: 300 },  // pico repentino
    { duration: '1m', target: 300 },
    { duration: '10s', target: 10 },   // baja de golpe
    { duration: '1m', target: 10 },    // ¿se recupera?
    { duration: '10s', target: 0 },
  ],
  thresholds: umbrales,
};

export const setup = setupComun;
export default sesionDeUsuario;

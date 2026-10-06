// PRUEBA DE ESTRÉS: sube por escalones hasta encontrar el punto de quiebre.
// Pregunta que responde: ¿cuántos usuarios simultáneos aguanta antes de degradarse o fallar?
// Mira en el reporte en qué escalón el p95 pasa de 500 ms o los errores pasan de 1 %.
// k6 run pruebas-carga/3-estres.js
import { setup as setupComun, sesionDeUsuario, umbrales } from './comun.js';

export const options = {
  stages: [
    { duration: '1m', target: 50 },
    { duration: '2m', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '1m', target: 200 },
    { duration: '2m', target: 200 },
    { duration: '1m', target: 400 },
    { duration: '2m', target: 400 },
    { duration: '1m', target: 800 },
    { duration: '2m', target: 800 },
    { duration: '2m', target: 0 },    // recuperación: ¿vuelve a la normalidad?
  ],
  thresholds: umbrales,
};

export const setup = setupComun;
export default sesionDeUsuario;

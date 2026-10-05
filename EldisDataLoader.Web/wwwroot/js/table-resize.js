window.makeTableResizable = () => {
    console.log("✅ Скрипт изменения ширины запущен!");
    const tables = document.querySelectorAll('.resizable-table');
    console.log("Найдено таблиц с классом resizable-table:", tables.length);

    if (tables.length === 0) {
        console.log("❌ Таблица не найдена. Проверьте класс в RawData.razor");
        return;
    }

    tables.forEach(table => {
        const oldResizers = table.querySelectorAll('.resizer');
        oldResizers.forEach(r => r.remove());

        const ths = table.querySelectorAll('th');
        ths.forEach(th => {
            th.style.position = 'relative';

            const resizer = document.createElement('div');
            resizer.className = 'resizer';
            resizer.style.cssText = 'position: absolute; top: 0; right: 0; width: 5px; cursor: col-resize; user-select: none; height: 100%; background: transparent; z-index: 10;';
            th.appendChild(resizer);

            let x = 0;
            let w = 0;

            const mouseDownHandler = function (e) {
                x = e.clientX;
                const styles = window.getComputedStyle(th);
                w = parseInt(styles.width, 10);
                document.addEventListener('mousemove', mouseMoveHandler);
                document.addEventListener('mouseup', mouseUpHandler);
            };

            const mouseMoveHandler = function (e) {
                const dx = e.clientX - x;
                th.style.width = `${w + dx}px`;
            };

            const mouseUpHandler = function () {
                document.removeEventListener('mousemove', mouseMoveHandler);
                document.removeEventListener('mouseup', mouseUpHandler);
            };

            resizer.addEventListener('mousedown', mouseDownHandler);
        });
    });
};
/**
 * Base64データからファイルをダウンロード
 */
window.downloadFileFromBase64 = (base64Data, fileName, contentType) => {
    try {
        console.log(`ダウンロード開始: ${fileName}`);

        // Base64をバイナリに変換
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);

        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType });

        // ダウンロード実行
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;

        // DOM操作でクリック実行
        document.body.appendChild(link);
        link.click();

        // リソースクリーンアップ
        setTimeout(() => {
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        }, 100);

        console.log(`ダウンロード完了: ${fileName}`);
    } catch (error) {
        console.error('ダウンロードエラー:', error);
        alert(`ファイルのダウンロードに失敗しました: ${error.message}`);
    }
};

// エラーUI制御
document.addEventListener('DOMContentLoaded', function () {
    const errorUI = document.getElementById('blazor-error-ui');
    if (errorUI) {
        const dismissButton = errorUI.querySelector('.dismiss');
        if (dismissButton) {
            dismissButton.addEventListener('click', function () {
                errorUI.style.display = 'none';
            });
        }
    }
});
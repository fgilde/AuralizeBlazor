// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions.Helper;

namespace AuralizeBlazor.Sample.Helper
{
    public partial class SectionSource
    {
        [Inject] protected IJsApiService JsApiService { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }

        /// <summary>
        /// Name of the example component. The matching source is loaded from the
        /// generated markdown file under wwwroot/example-codes/{Code}.md.
        /// </summary>
        [Parameter] public string Code { get; set; }

        [Parameter] public string Class { get; set; }

        [Parameter] public string GitHubFolderName { get; set; }

        [Parameter] public bool ShowCode { get; set; } = true;

        [Parameter] public bool NoToolbar { get; set; } = true;

        public string TooltipSourceCodeText { get; set; }

        private string ShowCodeExampleString { get; set; } = "Show code example";
        private string HideCodeExampleString { get; set; } = "Hide code example";

        private string _sourceCode;

        private async Task CopyTextToClipboard()
        {
            await JsApiService.CopyToClipboardAsync(_sourceCode);
        }

        public void OnShowCode()
        {
            ShowCode = !ShowCode;
            TooltipSourceCodeText = ShowCode ? HideCodeExampleString : ShowCodeExampleString;
        }

        protected override async Task OnInitializedAsync()
        {
            TooltipSourceCodeText = ShowCode ? HideCodeExampleString : ShowCodeExampleString;
            await LoadSourceCodeAsync();
        }

        private async Task LoadSourceCodeAsync()
        {
            if (string.IsNullOrEmpty(Code))
            {
                return;
            }

            try
            {
                _sourceCode = await HttpClient.GetStringAsync($"example-codes/{Code}.md");
            }
            catch (Exception)
            {
                // The generated source file might not exist yet (e.g. before the first build with Nextended.CodeGen).
                _sourceCode = null;
            }
        }
    }
}

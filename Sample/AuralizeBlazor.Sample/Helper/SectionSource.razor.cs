// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AuralizeBlazor.Sample.Helper
{
    public partial class SectionSource
    {
        [Inject]
        protected IJsApiService JsApiService { get; set; }

        [Parameter] public string Code { get; set; }
        [Parameter] public string Code2 { get; set; }

        [Parameter] public string ButtonTextCode1 { get; set; }
        [Parameter] public string ButtonTextCode2 { get; set; }

        [Parameter] public string Class { get; set; }

        [Parameter] public string GitHubFolderName { get; set; }

        [Parameter] public bool ShowCode { get; set; } = true;

        [Parameter] public bool NoToolbar { get; set; } = true;

        public string TooltipSourceCodeText { get; set; }

        private string ShowCodeExampleString { get; set; } = "Show code example";
        private string HideCodeExampleString { get; set; } = "Hide code example";

        private string CurrentCode { get; set; }
        private Color Button1Color { get; set; }
        private Color Button2Color { get; set; }

        private async Task CopyTextToClipboard()
        {
          //  await JsApiService.CopyToClipboardAsync(Snippets.GetCode(Code));
        }

        public void OnShowCode()
        {
            if (!string.IsNullOrEmpty(Code))
            {
                ShowCode = !ShowCode;
                if (ShowCode)
                {
                    TooltipSourceCodeText = HideCodeExampleString;
                }
                else
                {
                    TooltipSourceCodeText = ShowCodeExampleString;
                }
            }
        }

        RenderFragment CodeComponent(string code) => builder =>
        {
            try
            {
                var key = typeof(SectionSource).Assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains($".{code}.razor.html"));
                if (!string.IsNullOrEmpty(key))
                {
                    using (var stream = typeof(SectionSource).Assembly.GetManifestResourceStream(key))
                    using (var reader = new StreamReader(stream))
                    {
                        builder.AddMarkupContent(0, reader.ReadToEnd());
                    }
                }
            }
            catch (Exception)
            {
                // todo: log this
            }
        };

      
        protected override void OnInitialized()
        {
            CurrentCode = Code;
            Button1Color = Color.Primary;

            if (ShowCode)
            {
                TooltipSourceCodeText = HideCodeExampleString;
            }
            else
            {
                TooltipSourceCodeText = ShowCodeExampleString;
            }
        }

        private void SwapCode(string code)
        {
            CurrentCode = code;

            if (CurrentCode == Code)
            {
                Button1Color = Color.Primary;
                Button2Color = Color.Default;
            }
            else if (CurrentCode == Code2)
            {
                Button1Color = Color.Default;
                Button2Color = Color.Primary;
            }
        }
    }
}

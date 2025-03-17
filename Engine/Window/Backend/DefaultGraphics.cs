using System.IO.Compression;

namespace Pure.Engine.Window;

internal static class DefaultGraphics
{
    public static string PngToBase64String(string pngPath)
    {
        var img = new Image(pngPath);
        var bytes = Compress(img.Pixels);
        var base64 = Convert.ToBase64String(bytes);
        img.Dispose();
        return base64;
    }
    public static Texture CreateTexture()
    {
        var base64 = Convert.FromBase64String(DEFAULT_GRAPHICS_BASE64);
        var bytes = Decompress(base64);
        var img = new Image(208, 208, bytes);
        var result = new Texture(img) { Repeated = true };
        img.Dispose();
        return result;
    }

#region Backend
    private const string DEFAULT_GRAPHICS_BASE64 = "H4sIAAAAAAAAA+1by5Iju447i/n/X+47ExMVV8GDFyWl7XITm7REEgQhZVZv+p9/NP78L9Dvzlo9XU7Ks8v/Z0FnXWMKyls3b9cPlc/OSsU7/Vn9N4Od7TtQ9awa1bPmsfU7gWZlOY4jqWEe1t8sV3mZ+MzOj51dMgvzMOF/Sj/TplDrKl/n6XKe5ne1zN8UyDPlH5p/zUP7DkhLZ30CxeP6IB9RjOnv8Kt6hcyFZ1F11FnS5zpTx4Onsc7C1nU/jVff0F7qu8pBM6EZqxaU340rbam+6p/aR3mqd+Jf/d1Zq6fLSXl2+ZXnal1jCsxzpM/pZx4hfpSTxpgW5Q3Sknqn4kpbqq/6p/ZRnur9Kah6qhfsqXyvXr0aaLb1N5oF5dR419fav7M+geJxfZCPKMb0d/hVvULmwr/7ovXO0+U8ze9q0XoHyEOW4ziSmrrPfrNc5UHiD/OfeZzMwjxM+J/Sz7S9ElWH0qqeyo93Qs39E1/z1j0X7+Q7j1G805/Vfzvc+dU1izF/1320x+KMx+l9Ip7qVJoRVF4yfz0HtJfqT3sjnsQPpEvlMw40g0IyH9NfZ2V9lZ9pftfPysf0JfwIiX6Wj3q5PTWP42f5bF7nAZqd6XD8rk79Rt4ketL+bE7nVTq/05/yISC9qQbH1/Vfza/geqIc5h+an3lRa5Sfjl/VO/8SfYn+GmdamOeMq/ZGe+l8TJfSn9Y7/92egvI/4We+v2q+k/4d/9R9eFL/K/jdfJ065v+6zzSqHOZHghP/Um43H+NP/WPznM530r/jX+V9lf5X8Lv5OnXM/3WfaVQ5zA8H1R/1ZvodJ/KE8aF9xoH0Kt2dnigH9WHzMy9qjfLT8at651+iL9Ff40wL85xx1d5oL52P6VL60/qT87vhP/Knw4+Q6Gf5qJfbU/M4fpbP5nUeoNmZDsfv6tRv5E2iJ+3P5nRepfOn+lV94o/TnM6d8NzwoxtPdSrNCO583Pz1HNBeqj/tjXgSP5Aulc840AwKyXxMf51VeT0YDPbAvi0/sTVn3XtV3K3frQ/5+SnzrbHKjWKJHsTR9eNWPPVP8Z3M5/hP53NI+++ev6tP46k2xqGQaFfrut+Ju/mSONLV0af8S+In85/yI45Xzn/qv8OpP2wepJFpU/FTf9L+jDuZ38UTf5LZVNzNtzv/0/ypf4zDxWse0876OyT6kvmUTsSXxk/9Sfsz7mR+F0/8SWZTcTff7vyn/C7/hn6FtL86f4S0Po2n2hiHQqJdret+J+7mS+JIV0ef8i+Jn8x/yo84uvM/HXfrd+v7U/BJ862xyo1iiR7E0fXjVjz1T/GdzjcYDPpw3yNXx/JUfcL/k5doV31UbqqRzdrld1yn/Eg/ijG+W/WOe81TNa+Iu7XT7/rd6p/WVri4y0n0p31Q7o5fKFb52Nr1f3eczdrZf0Jf6qfSqvhu6j/1x9WfxtO9hDOZJT2PChRPer477jxz+7v9WU31UPElWl0u0sf6O/9u8af9aw6rV5ocFH/qgZsv1e9muR1HszJtiQdp/2StNChtiEfFVR3Tl/q760+n/pQ/3Ut6Mt9S7sql9KM+6fxMY8qv5kvm7XjDNCr93ZxUd63v6u/Ml9S7dTpTqifRr/p0/GE+qxm6/tce3XrXP/FS5Xf2durdfne92zfV/4eA8dS4q2X8tT/S7niS+XY1Jlxuxo7/u/pRvfPuZ+9kPtYj6a/yEt6ufwlXqnEwGNzDE+/aLufOu5/m7+YldfPNeuYefRLY36bu/ejkd+oVL6r/yX363FJ+NxfTq2ZTOaf1SM9O/3R+tvcp8WTGTn06v+PYrU/7p7OzfZSj+rMeSV/kSTrf03E2c9e3xJeb+k/z1TxsvlP9O/6w/m6mHX0dftcv9SLRj3hO53sizmZYsTP/qb6khs3SmWGN3dSvdO3WKx41e6o/4Vd5znPHX7Wiva5/T8cdOv4muUqP86/mqVn+FCS17/C/E1d7zocb/MzLtFeHH/Hc8rcC5aMcNROD06T2E42JdsSZzKNqXY3yj2ljOSl/Uu/mSLgdV9I74erO5zi7PRN+B9dfeaPynYe1L6p3nF1/WX83n/JkMBj0kLxXLO7ezfT9RbGd2qRe5ezMh2p+QzyZKYnv8p/2T89ktz7Vp7Q6nmSvw5/oOem37iXePFH/KfFkjThv8bteSU2yVvpP+rv7keyjnE79uo9mRehoUhoYp+LfyUe/Vb3id/Xd+Lp/Q7/jR7GT/oq/+pn2T883uR9uv/ZVta6/6uHmVf2TudI40ox0sXhSX/un9XXP1avfanbV3/Hf7I9q6m/Go/or/92ZKR61z+KKx/mT1judzpvOHIwf/WZ7bM36r3Gm6ZY+FU/6u/kS/afx3f7Ofzab06d0qfumapJ8V5/ys55srztP/e38Pq1ncXQebu6b/dP1SX+nP9G32x/FWV8Uq3ksxmZiPdhsSX3Cr/p2+WtM+ci0uPrdeN3f5f+t9bfmT/UNBoPBN8N961TcfSvd95R9xzv174xX7Sgvjc/fnd+H9P6mPLux3f6fpA/l1njlYXtunsFnwN0PF0viKif5vrt6p+Hk/Vg1phzqfTj5++T672pXOen341PjVfuJ/6zPqb9JnPE7Dafnr7iT+m685v0h6MTVDKf+prHT8/kEfSi3xtn5pXoGnwV3P1wsiasc9E53652Gk/dj1ZhyqPcB8bj44LOR3J3d+3/69yG5u+n93qnvxtG7g5DGB4PBYPA+oO/x6Tca/Q1IapK/R2lfxrv7tzDRv1P3W1DPlO05DvZviE7/DlC/GzzrXteHtE/yO+2LtCbxG2eTaN9999L3+51Q86d3J/l2pRq6cHfnBJ27ob4fyfmz+O07fiu2xt37ieLIm+78a01yz24jeT/SO+T6PHn/Uu7Kc8LfgeJ1PVNvVV7C4bh3tNZ9FncaUW9We3pWKTo9Xa7zdvf8EqT37JQf9XD3S8XT38lcJ/7uxpLcG+9P4n/9/Sqc6q95ar5URwfqPE540P7Oe9rN7c7jNKXxpMduHvp2oJqTd+ydcO83i3073Hnv1N/Q0M1Jvnnqjte83T67e2t/xv9uuO9A4iv7fibnMxgMzqDev53alOPGt/cnr7OfAn3PEg03v1W7s93SkJxtPSN2n3Z0qjuQ/P1Qmlj/jtYb95/FE35Vm+Sd3qOEu8tx++4yfudrenbJ/ezU1/5IT737Oz1urFn/rj4GleNqXd5pfcJxEkfnntQncyXeKy52F5D25I4ofcndq/nuqbxlvEwfA+NgWmtO5er2d7md+s7+q+rTWLd/cq7udwp3jiiO7mfnPlRedvfYk2lI50q0q5z6m/VQa6cxnUPVurzT+oTjJK48PtGE8rs1aW16x3Z6qvu5PuvvylXnYLyduVUe06r0Kv07sXQGx69qk7yk/yl36mUav4ndM0L3dbcvu3vuXlV0eyRzsx6uf6JPaUk9VXmdGXfia1533/m71ifervUp/y0o/qf6onnXWH0qfUmfyp/66+bvaHrq/AaDvxHuu/j0d/NJqO9jzVMcT+nr9qh5ne/2ybf55H785vuTIPH2nR6cnM+N9yeJnyLh72pP9tnvulefN+Lobv3G98zdv5r3WnVZ7/Td2H1/0pxdPPn+1PNlZ5re/d0n0os0/tb35zTnE5G8Ozd6nPqT+N+NJ/dZaa93W/G6ONPKNKlZPw3u7N/97vzW71IHzv8dDvZtV+9Rres8UV+k7dven1Mk95vF3Vm4+jT+6Xjy/enkO47b62/AjbupclwMPTv47e/O/8H5s8vzB0DFHQfreRIfDBDQ3Z17NEBw9+Lpe/M39195UQ8Xv9FffRue/na8u/8NfML9PYn/Zvzt78834N3vz0+Pk/hpb8bP7k/dV/dMxRPOtOdu/x2s2tGe8lj1R7ms7wmS+VR+h4vxu2ftUb1B/dnaPVXvJAf1r7muh+vL4ikX4lXno/JSHlXv/EI9UA6Ko1zV4yn/WG6HS/Vgz45/tb7mojWqOe3v+FQ9g8vtcCG96mxUXsqj6p1fiN/lKD5W/zSqZ7fjqYadWBJPeu/Oh84f7as8VOPiOz07NTuctX591j3ln+qP+FT9K1A9ux1P+p/mvNqzW6h35XY86V/v383434BPeH/+1v5/+/vz7v6DwWDwqajftxRT/++/y931Lf27v9/t32+v361d6/85wK36k+du7Vp/4t9pf8ST/L6l/8b5ndafPE/9v+Hfaf07/Tt5nnh4w78b/U/r/+bz35kbzX9a/07/dmtXDafz79af9p/6z/j+7OJW/Tufp/7t9kY6Or9PfB/8Fyfn/7fXM650fUv/7u/BYDD4BOx+/z6h/mfdfSINp/VrzK1v+qewUzPI0D03difeXf/zu/PcqWG9d9Z1/m5/NH+3/2Af+oZqoPrT/vcn9P1Pn8qP1L+d3u63yndep/o/uR55gdas1j1TnUp/Moub8531Pxw7z6ph179dHapenZPyrKuf1Se+P1H/w3Famzxvomo4qX8lfnrX5w5Pd54Tv9ce63qnflcP8zLlUvXvwOphmn8Dp5wn857Wo2e3/+4syLtOfa255efuua0+ntSf6D+t7XB0/XI+nNa/Azu+rbUns7D6jm+1RnErrWlPVd/lQfW7OK3f4djxTPlwWr8z72n9+ny1jlqz6tn1faceaUlmdJypB07XCf/KcxLf7ZF4dGO+HW1o/ykNrn991hznzyvOV3Erfe7cV33uPHbWK/caT96t9P6i3h0gftQP5ST5TH8yH5sN1SqOzuxJfPV9jaN9p89pVvHduZlG17vOh/hqHfvt9Nf9+hutuz4gvV0+5GHHH9QPnU2H382B+iBdbm61PumfxBN/2DrVr2ZX/Wse+630s77JHutRa1xOMtsT/Kpf5UdenD7T+VhfxIHyK9C+y2XaUf90PjUj4ndamX7XP/WfeaJ+s/xO/zSH9VY613jK0dGPeqi5kB7323mT/kZr5FMaR3N29Hf6u1nQb6YZ6VMxp0vtV+3KQxZn/iH/VY7iZ345MF42A6tn/ZPZlCYG1DudSWnY0a90JbNWnrT/qX7XH+U7bjdDwpHGk/4n/A63cp0/jjfRz7idfx0dXX0/v2tMnZHr2107/iQ+GAzO4N49lrf7XUo11e/S7vcx6bMTR9/X6peKq17s9y4QR0fbT36nvsvvtDtvXW/WX+laz8DFlD42k5uZxVG/RBuqYTOuGpjHSm8nrvon5+dyGJI65jPyL9GYzpf4ucvh9hMfXa8ESneN1d+qJ+NN+qI4WqO5d+Ksv4qr32hGdTYop9Yi35F3TIf6vesJ08g8cfN1ONx+9YnxMyReOP2qX+K505vO4mrR704c8bo4ykFrd3b1PFTtmoPOUWlK+Tt+uPqb/jg9yJdOPevnoLQnc6wcjNfxo5lqHVp3uNRvdf5qBhRnM7A4m4XVpU81F+uP5mP6Un86c6IeLKfrT9Xk4DxzvdSeWrsZFR/iTjSq2dKa1FfWbzfe8YDF2P3p8rPzS/SjXBevOtJ9BXVf6lrFdnp2Yz9xdVYuzvqge6HOaKfeee2QeHOrVs2HPHZ+M45UX6pRrdV+2l/NwHI6vXbvxmAw4HDfL5WL4p2e3Zji29Gxg+7s9buFnisnWtd9Fu/w13lYvuNgmpgfJ1CcyJMuN5p9V1Pdd/xpb+dBqpmd944HO2D+JLE1jrS+Qv9vQfUK/f5ZO9/UXa9n17mLaI3OWL07aS3iUTE26079yZ10Pql4PZ+6Tp9oxm4dm20nL9XH9hLfE693ONB+4sEN3sqv5mOxpO/av2pBc6i5T+Jon+mv8cEZ6vmjveR+Ih5V39WnejAd6z5bp/N3/ElnQHmpL4q7zod6IP1snT6RhrQO9VbxJCf1z+lne+h33Uv6sx7qnFIgfsaj9tma6atPlI80Kv0nPigoPqRT1dccVHNb/7dAedK5I5UL3cuOJoYb+hP+G73T3F2k/ZkW9aznh86TebTDz2Zg3nd4unDzoVylj+Wxc6n1zofBYPC5QO/u0+9z/faw74/S5r4/Tn8nvsuVfH87c57qOc39JiT3T63XWnV2Xd61Tt0rpd/dHQbE7/qncZff7aP0I846H9OCctU5Oi++Een9Y+t1bzeWrNXZsRx0r1B+Ev9Z1yeKo/6oHoHFVC3qy3Qgj5B3rC/LYfXfDHZfXJzlIA/d2Sl+15vlqN6M08VXTqXf+Yvyao3bR31212q+Gq+5Sfybwe6Li6t7shtzehyH0490dOI/6/pEcTWfy0s50Py768qH1qw/0vg3Ad0XFmc56747/w6309bRj3SgWDrvSdxpYDHGXedP10iL049mWfmZD9+M6i+Ls/W6r/zt8iZng+4Hy2H8CMm8O/GOPjUT49tZux6qnu0NBoNBCvVdqd+ozvcQ1ad1a239nfLWOVTeYLCD7t/wtQ6t2R13/0ZR/9ZAv91eEhsMTpHc7XWt4jXn1vuD9l2+qx8MTqHuN3oP6j7iSfnTOHoyjazW6R8MdpB+s5NalYfejRpT76/TzPiT+GCwg+R+pncvucNdLexdcn9zmP55hwY34e569zu/0yvRsurp8sz7MxgMBoPBYDAYDAaDwWAwGAwGg8FgMBgMBoPBYDAYDAaDwWAwGAwGg8FgMBgMBoPBYLAP9X8zd/7vZ7f+t+Lpub7Vt2+Du+MncfZ/wtc69n+sn4473PLF6XuKf/AaJP6vcXRuKr5yqN6sv4q7+6nWbK/GlK4OL9Jza27FM7gL5v1NqN5Ki9Pq5lD9FL/Tusvt1ruzo7jyfXAPyPfbUL2VDqYzqUX16Il6JFq7cZfjPEs8TX0fPIfE/3qP1f1lZ1p/34qvfWtu1YTmUjOrHBd3ec53F+/mDZ6B8/4kru5Yeu9OdKl6h1NfnJan+QevwekZ3roDvw1Pz/Wtvv127LwP9d8IfwhQPnuHXnH/Tv5GuXo3n+NP4oPPQ3Knkvte3xfGpfpkij1239FUt1t39qrmeYd+Fzrf1WTv5zfjUjo6sfRdYO91h7P7rai92XPw+/GnIImzfPf9TL+/6s5334HKq2ZxXjBNKlZ71Tl2+g8+B+7c0nNVd0rtJ/eH1XX3djhdjuJE703aZ/A7kLwXyfdwzem8P7VH2i/h7ebtxNXM6l1Dud25B+/Hev7ufqTn331/UH33/Unfb6ch7enq0r0b/Qefgd3zdX8/0ndj5915JU6/H4Pvxend/u3xwWAXP3eqPv+W+GCwi/Uuod91T93FpP5d8fq3Z96hwQ3cvr8rnuBH8dqzUz8YnIDdI3T/bsR3/345nl19g8Ep2N189dq9K0+tB4NToH8D/U3xwWAwGPwu/DH4dH7W5xbvTf5Tbd36+Vt9B+ru3rjfjl/V7c6Q9r/Fn2h3fZ+OJxoHGLv3i8Uqz+79cvcB6XSz7eh3/E/qRzpVjuulNCl9A47k3pzcP3V+N+7fN+tP44kep4nVDDS++f59s/5TfQpJzuD/8Yr7l5x93VNnyHS62Xb0O/4T/V3vUFxpSjXv5Aw8bt2/XX5VtztD2v8Wv9Lu5ldI6pVmN1PHg0Efyfl+Mj/rc4v3lP90/pP6J3z4RjhfJz7xieP4ul6fdY/9vlFfNXb5GTr1J/xOf+LfCb+rT+Z7Zzzx72T+jv875/P0fIn+U/9P+F19Elfz39CPkMZP57vhn/Lnhn/rXte/zsxqn/G/Iq70f4K+p+Ppvdi5v0zPLf3fcL9O7p+b/x8A1IudbzLfk3GW15nfxVGPtL/Tv/bY9VfVu2dnT+Xu6k/4O3Xd/on+k/gN/077V7D6p/xBOU/od/1316xnUu+ep/zJ/Ik/Fd3zUb8R98QnPnEfHwwGe0Dv1vqOfXq8zqJmrL+ZD8or5+U7+6u6xLuqM+lf87v1v10/0oZ03IwzvTtx5l86n6vvxJku91vVp3FXm86ezNjlrzk7+tP1rn70VDm1H/Pg0+Ms59Z8jj85T3dm6++65/iRRtaPaXfzJ7O4WlW/xhK/HOeO/p28tVc9O7RGHr2iPt13/E/FlddpXM2XnGWtd+dc9x3HLn8nnszr9DOO2+eF5qr4hjjLcf7v1LM9FO/2Z/XpPtOf5qjaG/HOXEx/7YXirkc3jnxb+6v9NF7zbsbRHGx+FVfepfyKw8XT2M587IxUTof/VpzNh85a5aj47fND3q5zfnqc5SgPWEzVOm8/tf+ufy7emf8k/hv0DwZ/Ff7nHv4DssLtrwCkAgA=";
    private static byte[] Compress(this byte[]? data)
    {
        try
        {
            if (data == null || data.Length == 0)
                return [];

            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
            {
                gzipStream.Write(data, 0, data.Length);
            }

            return memoryStream.ToArray();
        }
        catch (Exception)
        {
            return data ?? [];
        }
    }
    private static byte[] Decompress(this byte[]? compressedData)
    {
        try
        {
            if (compressedData == null || compressedData.Length == 0)
                return [];

            using var compressedStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gzipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        catch (Exception)
        {
            return compressedData ?? [];
        }
    }
#endregion
}